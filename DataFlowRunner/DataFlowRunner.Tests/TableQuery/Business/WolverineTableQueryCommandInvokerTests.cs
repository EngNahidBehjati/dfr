using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.TableQuery.Business;
using DataFlowRunner.TableQuery.Commands;
using DataFlowRunner.TableQuery.Exceptions;
using DataFlowRunner.TableQuery.Options;
using DataFlowRunner.TableQuery.Reports;
using DataFlowRunner.TableQuery.WolverineIntegration.Abstractions;
using DataFlowRunner.TableQuery.WolverineIntegration.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wolverine;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;

namespace DataFlowRunner.Tests.TableQuery.Business;

public sealed class WolverineTableQueryCommandInvokerTests
{
	private readonly IOptions<TableQueryInvokeTimeoutOptions> _invokeTimeoutOptions;
	private readonly TimeSpan _expectedTimeout = TimeSpan.FromMinutes(1);
	private readonly IWolverineTableQueryCommandMapper _commandMapper;

	private readonly IWolverineTableQueryReportMapper<
		ExecuteTableQueryWolverineReport, ExecuteTableQueryReport> _executeReportMapper;

	private readonly IWolverineTableQueryReportMapper<
		CalculateRowCountWolverineReport, CalculateRowCountReport> _rowCountReportMapper;

	private readonly ILogger<WolverineTableQueryCommandInvoker<
		ExecuteTableQueryCommand, ExecuteTableQueryReport, ExecuteTableQueryWolverineReport>>
		_executeTableQueryCommandLogger;

	private readonly ILogger<WolverineTableQueryCommandInvoker<
		CalculateRowCountCommand, CalculateRowCountReport, CalculateRowCountWolverineReport>>
		_calculateRowCountCommandLogger;

	private readonly ILanguageDictionaryProvider _languageDictionaryProvider =
		LanguageDictionaryProvider.Instance;

	public WolverineTableQueryCommandInvokerTests()
	{
		_invokeTimeoutOptions = MicrosoftOptions.Create(new TableQueryInvokeTimeoutOptions
		{
			TimeoutInSeconds = _expectedTimeout
		});
		_commandMapper = Substitute.For<IWolverineTableQueryCommandMapper>();
		_executeReportMapper = Substitute
			.For<IWolverineTableQueryReportMapper<ExecuteTableQueryWolverineReport, ExecuteTableQueryReport>>();
		_rowCountReportMapper = Substitute
			.For<IWolverineTableQueryReportMapper<CalculateRowCountWolverineReport, CalculateRowCountReport>>();
		_executeTableQueryCommandLogger = Substitute
			.For<ILogger<WolverineTableQueryCommandInvoker<ExecuteTableQueryCommand,
				ExecuteTableQueryReport, ExecuteTableQueryWolverineReport>>>();
		_calculateRowCountCommandLogger = Substitute
			.For<ILogger<WolverineTableQueryCommandInvoker<CalculateRowCountCommand,
				CalculateRowCountReport, CalculateRowCountWolverineReport>>>();
	}

	[Fact]
	public async Task InvokeQueryCommandAsync_ShouldReturnReport_WhenCommandIsValid()
	{
		// Arrange
		var command = CreateExecuteTableQueryCommand();
		var expectedReport = CreateExecuteTableQueryReport(command, isSucceeded: true, error: null);
		(IServiceScopeFactory scopeFactory, IMessageBus messageBus) = CreateScope();
		ExecuteTableQueryWolverineReport wolverineReport = SetupExecuteQuery(messageBus, command, expectedReport);
		var sut = CreateExecuteSut(scopeFactory);

		// Act
		ExecuteTableQueryReport result = await sut.InvokeQueryCommandAsync(command, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(expectedReport);
		await AssertExecuteQueryInvokedAsync(messageBus, command, wolverineReport);
	}

	[Fact]
	public async Task InvokeQueryCommandAsync_ShouldThrowTableQueryExecutionFailedException_WhenReportIsFailed()
	{
		// Arrange
		var command = CreateExecuteTableQueryCommand();
		var failedReport = CreateExecuteTableQueryReport(
			command, isSucceeded: false, error: new GraphExecutionError("boom", "Stack trace"));
		(IServiceScopeFactory scopeFactory, IMessageBus messageBus) = CreateScope();
		ExecuteTableQueryWolverineReport wolverineReport = SetupExecuteQuery(messageBus, command, failedReport);
		var sut = CreateExecuteSut(scopeFactory);

		// Act
		Func<Task> act = async () => await sut.InvokeQueryCommandAsync(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<TableQueryExecutionFailedException>();
		await AssertExecuteQueryInvokedAsync(messageBus, command, wolverineReport);
	}

	[Fact]
	public async Task InvokeRowCountQueryAsync_ShouldReturnReport_WhenCommandIsValid()
	{
		// Arrange
		var command = CreateCalculateRowCountCommand();
		var expectedReport = CreateCalculateRowCountReport(command, rowCount: 100L, isSucceeded: true, error: null);
		(IServiceScopeFactory scopeFactory, IMessageBus messageBus) = CreateScope();
		CalculateRowCountWolverineReport wolverineReport = SetupRowCountQuery(messageBus, command, expectedReport);
		var sut = CreateRowCountSut(scopeFactory);

		// Act
		CalculateRowCountReport result = await sut.InvokeQueryCommandAsync(command, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(expectedReport);
		await AssertRowCountQueryInvokedAsync(messageBus, command, wolverineReport);
	}

	[Fact]
	public async Task InvokeRowCountQueryAsync_ShouldThrowTableQueryExecutionFailedException_WhenReportIsFailed()
	{
		// Arrange
		var command = CreateCalculateRowCountCommand();
		var failedReport = CreateCalculateRowCountReport(
			command, rowCount: 0, isSucceeded: false, error: new GraphExecutionError("failed", "Stack trace"));
		(IServiceScopeFactory scopeFactory, IMessageBus messageBus) = CreateScope();
		CalculateRowCountWolverineReport wolverineReport = SetupRowCountQuery(messageBus, command, failedReport);
		var sut = CreateRowCountSut(scopeFactory);

		// Act
		Func<Task> act = async () => await sut.InvokeQueryCommandAsync(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<TableQueryExecutionFailedException>();
		await AssertRowCountQueryInvokedAsync(messageBus, command, wolverineReport);
	}

	private ExecuteTableQueryWolverineReport SetupExecuteQuery(
		IMessageBus messageBus,
		ExecuteTableQueryCommand command,
		ExecuteTableQueryReport report)
	{
		var wolverineCommand = new ExecuteTableQueryWolverineCommand
		{
			GraphExecutionId = command.GraphExecutionId,
			UserId = command.UserId,
			NodeId = command.NodeId,
			OutputPortId = command.OutputPortId,
			IncludeSchema = command.IncludeSchema,
			ODataQuery = command.ODataQuery
		};
		var wolverineReport = new ExecuteTableQueryWolverineReport
		{
			GraphExecutionId = report.GraphExecutionId,
			NodeId = report.NodeId,
			PortId = report.PortId,
			TableSchema = report.TableSchema,
			Rows = report.Rows,
			IsSucceeded = report.IsSucceeded,
			Error = report.Error
		};
		_commandMapper.ToWolverineCommand(command).Returns(wolverineCommand);
		messageBus.InvokeAsync<ExecuteTableQueryWolverineReport>(
				wolverineCommand,
				Arg.Is<DeliveryOptions>(options => options.PartitionKey == command.GraphExecutionId.ToString()),
				CancellationToken.None,
				_expectedTimeout)
			.Returns(Task.FromResult(wolverineReport));
		_executeReportMapper.ToReport(wolverineReport).Returns(report);
		return wolverineReport;
	}

	private CalculateRowCountWolverineReport SetupRowCountQuery(
		IMessageBus messageBus,
		CalculateRowCountCommand command,
		CalculateRowCountReport report)
	{
		var wolverineCommand = new CalculateRowCountWolverineCommand
		{
			GraphExecutionId = command.GraphExecutionId,
			UserId = command.UserId,
			NodeId = command.NodeId,
			OutputPortId = command.OutputPortId,
			ODataQuery = command.ODataQuery
		};
		var wolverineReport = new CalculateRowCountWolverineReport
		{
			GraphExecutionId = report.GraphExecutionId,
			NodeId = report.NodeId,
			PortId = report.PortId,
			RowCount = report.RowCount,
			IsSucceeded = report.IsSucceeded,
			Error = report.Error
		};
		_commandMapper.ToWolverineCommand(command).Returns(wolverineCommand);
		messageBus.InvokeAsync<CalculateRowCountWolverineReport>(
				wolverineCommand,
				Arg.Is<DeliveryOptions>(options => options.PartitionKey == command.GraphExecutionId.ToString()),
				CancellationToken.None,
				_expectedTimeout)
			.Returns(Task.FromResult(wolverineReport));
		_rowCountReportMapper.ToReport(wolverineReport).Returns(report);
		return wolverineReport;
	}

	private async Task AssertExecuteQueryInvokedAsync(
		IMessageBus messageBus,
		ExecuteTableQueryCommand command,
		ExecuteTableQueryWolverineReport wolverineReport)
	{
		_commandMapper.Received(1).ToWolverineCommand(command);
		await messageBus.Received(1).InvokeAsync<ExecuteTableQueryWolverineReport>(
			Arg.Is<ExecuteTableQueryWolverineCommand>(actual =>
				actual.GraphExecutionId == command.GraphExecutionId
				&& actual.UserId == command.UserId
				&& actual.NodeId == command.NodeId
				&& actual.OutputPortId == command.OutputPortId
				&& actual.IncludeSchema == command.IncludeSchema
				&& ReferenceEquals(actual.ODataQuery, command.ODataQuery)),
			Arg.Is<DeliveryOptions>(options => options.PartitionKey == command.GraphExecutionId.ToString()),
			CancellationToken.None,
			_expectedTimeout);
		_executeReportMapper.Received(1).ToReport(wolverineReport);
	}

	private async Task AssertRowCountQueryInvokedAsync(
		IMessageBus messageBus,
		CalculateRowCountCommand command,
		CalculateRowCountWolverineReport wolverineReport)
	{
		_commandMapper.Received(1).ToWolverineCommand(command);
		await messageBus.Received(1).InvokeAsync<CalculateRowCountWolverineReport>(
			Arg.Is<CalculateRowCountWolverineCommand>(actual =>
				actual.GraphExecutionId == command.GraphExecutionId
				&& actual.UserId == command.UserId
				&& actual.NodeId == command.NodeId
				&& actual.OutputPortId == command.OutputPortId
				&& ReferenceEquals(actual.ODataQuery, command.ODataQuery)),
			Arg.Is<DeliveryOptions>(options => options.PartitionKey == command.GraphExecutionId.ToString()),
			CancellationToken.None,
			_expectedTimeout);
		_rowCountReportMapper.Received(1).ToReport(wolverineReport);
	}

	private WolverineTableQueryCommandInvoker<ExecuteTableQueryCommand, ExecuteTableQueryReport,
			ExecuteTableQueryWolverineReport> CreateExecuteSut(
		IServiceScopeFactory scopeFactory)
	{
		return new WolverineTableQueryCommandInvoker<ExecuteTableQueryCommand, ExecuteTableQueryReport,
			ExecuteTableQueryWolverineReport>(
			scopeFactory,
			_invokeTimeoutOptions,
			_executeTableQueryCommandLogger,
			_languageDictionaryProvider,
			_commandMapper,
			_executeReportMapper);
	}

	private WolverineTableQueryCommandInvoker<CalculateRowCountCommand, CalculateRowCountReport,
			CalculateRowCountWolverineReport> CreateRowCountSut(
		IServiceScopeFactory scopeFactory)
	{
		return new WolverineTableQueryCommandInvoker<CalculateRowCountCommand, CalculateRowCountReport,
			CalculateRowCountWolverineReport>(
			scopeFactory,
			_invokeTimeoutOptions,
			_calculateRowCountCommandLogger,
			_languageDictionaryProvider,
			_commandMapper,
			_rowCountReportMapper);
	}

	private static (IServiceScopeFactory ScopeFactory, IMessageBus MessageBus) CreateScope()
	{
		IMessageBus messageBus = Substitute.For<IMessageBus>();
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSingleton(messageBus);
		IServiceScopeFactory scopeFactory = serviceCollection.BuildServiceProvider()
			.GetRequiredService<IServiceScopeFactory>();
		return (scopeFactory, messageBus);
	}

	private static ExecuteTableQueryCommand CreateExecuteTableQueryCommand()
	{
		return new ExecuteTableQueryCommand
		{
			GraphExecutionId = Guid.NewGuid(),
			UserId = Guid.NewGuid(),
			NodeId = Guid.NewGuid(),
			OutputPortId = Guid.NewGuid(),
			IncludeSchema = true,
			ODataQuery = new Dictionary<string, string>()
		};
	}

	private static CalculateRowCountCommand CreateCalculateRowCountCommand()
	{
		return new CalculateRowCountCommand
		{
			GraphExecutionId = Guid.NewGuid(),
			UserId = Guid.NewGuid(),
			NodeId = Guid.NewGuid(),
			OutputPortId = Guid.NewGuid(),
			ODataQuery = new Dictionary<string, string>()
		};
	}

	private static ExecuteTableQueryReport CreateExecuteTableQueryReport(
		ExecuteTableQueryCommand command,
		bool isSucceeded,
		GraphExecutionError? error)
	{
		return new ExecuteTableQueryReport
		{
			GraphExecutionId = command.GraphExecutionId,
			NodeId = command.NodeId,
			PortId = command.OutputPortId,
			TableSchema = new List<TableQueryReportColumn>(),
			Rows = new List<TableQueryReportRow>(),
			IsSucceeded = isSucceeded,
			Error = error
		};
	}

	private static CalculateRowCountReport CreateCalculateRowCountReport(
		CalculateRowCountCommand command,
		long rowCount,
		bool isSucceeded,
		GraphExecutionError? error)
	{
		return new CalculateRowCountReport
		{
			GraphExecutionId = command.GraphExecutionId,
			NodeId = command.NodeId,
			PortId = command.OutputPortId,
			RowCount = rowCount,
			IsSucceeded = isSucceeded,
			Error = error
		};
	}
}
