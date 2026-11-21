using System.Collections.ObjectModel;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.TableQuery.Abstractions;
using DataFlowRunner.TableQuery.Business;
using DataFlowRunner.TableQuery.Commands;
using DataFlowRunner.TableQuery.Exceptions;
using DataFlowRunner.TableQuery.Models;
using DataFlowRunner.TableQuery.Reports;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.TableQuery.Business;

public sealed class TableQueryFacadeTests
{
	private readonly IDataFlowExecutionUserIdProvider _dataFlowExecutionUserIdProvider;
	private readonly ITableQueryCommandInvoker<ExecuteTableQueryCommand, ExecuteTableQueryReport> _tableQueryCommandInvoker;
	private readonly ITableQueryCommandInvoker<CalculateRowCountCommand, CalculateRowCountReport> _rowCountQueryCommandInvoker;
	private readonly INodeExecutionStateValidator _nodeExecutionStateValidator;
	private readonly ITableQueryResponseCreator _tableQueryResponseCreator;
	private readonly TableQueryFacade _sut;

	public TableQueryFacadeTests()
	{
		_dataFlowExecutionUserIdProvider = Substitute.For<IDataFlowExecutionUserIdProvider>();
		_tableQueryCommandInvoker = Substitute.For<ITableQueryCommandInvoker<ExecuteTableQueryCommand, ExecuteTableQueryReport>>();
		_rowCountQueryCommandInvoker = Substitute.For<ITableQueryCommandInvoker<CalculateRowCountCommand, CalculateRowCountReport>>();
		_nodeExecutionStateValidator = Substitute.For<INodeExecutionStateValidator>();
		_tableQueryResponseCreator = Substitute.For<ITableQueryResponseCreator>();

		_sut = new TableQueryFacade(
			_dataFlowExecutionUserIdProvider,
			_tableQueryCommandInvoker,
			_rowCountQueryCommandInvoker,
			_nodeExecutionStateValidator,
			_tableQueryResponseCreator,
			LanguageDictionaryProvider.Instance);
	}

	[Fact]
	public async Task QueryTableAsync_ShouldReturnMappedResponseDto_WhenAllOperationsSucceed()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var outputPortId = Guid.NewGuid();
		const bool includeSchema = true;
		var queryParameters = new Dictionary<string, string>
		{
			{ "filter", "value" }
		};

		var userId = new UserId(Guid.NewGuid());
		var expectedExecutionId = new DataFlowExecutionId(dataFlowExecutionId);
		var expectedCommand = new ExecuteTableQueryCommand
		{
			GraphExecutionId = dataFlowExecutionId,
			UserId = userId.Value,
			NodeId = nodeId,
			OutputPortId = outputPortId,
			IncludeSchema = includeSchema,
			ODataQuery = queryParameters
		};
		var expectedReport = new ExecuteTableQueryReport
		{
			GraphExecutionId = dataFlowExecutionId,
			NodeId = nodeId,
			PortId = outputPortId,
			TableSchema = new List<TableQueryReportColumn>(),
			Rows = new List<TableQueryReportRow>(),
			IsSucceeded = true
		};
		var expectedResponseDto = new TableQueryResponseDto
		{
			TableSchema = new List<TableColumnDto>(),
			Rows = new List<RowDto>()
		};

		_dataFlowExecutionUserIdProvider.GetDataFlowExecutionUserId(expectedExecutionId).Returns(userId);
		_tableQueryCommandInvoker.InvokeQueryCommandAsync(expectedCommand, CancellationToken.None)
			.Returns(Task.FromResult(expectedReport));
		_tableQueryResponseCreator.CreateResponseDto(expectedReport.TableSchema, expectedReport.Rows!)
			.Returns(expectedResponseDto);

		// Act
		TableQueryResponseDto actual = await _sut.QueryTableAsync(
			dataFlowExecutionId,
			nodeId,
			outputPortId,
			includeSchema,
			queryParameters,
			CancellationToken.None);

		// Assert
		actual.Should().BeSameAs(expectedResponseDto);
	}


	[Fact]
	public async Task QueryTableAsync_ShouldThrowTableQueryExecutionFailedException_WhenRowsIsNotProvided()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var outputPortId = Guid.NewGuid();
		const bool includeSchema = true;
		var queryParameters = new Dictionary<string, string>
		{
			{ "filter", "value" }
		};

		var userId = new UserId(Guid.NewGuid());
		var expectedExecutionId = new DataFlowExecutionId(dataFlowExecutionId);
		var expectedCommand = new ExecuteTableQueryCommand
		{
			GraphExecutionId = dataFlowExecutionId,
			UserId = userId.Value,
			NodeId = nodeId,
			OutputPortId = outputPortId,
			IncludeSchema = includeSchema,
			ODataQuery = queryParameters
		};
		var expectedReport = new ExecuteTableQueryReport
		{
			GraphExecutionId = dataFlowExecutionId,
			NodeId = nodeId,
			PortId = outputPortId,
			TableSchema = new List<TableQueryReportColumn>(),
			Rows = null,
			IsSucceeded = true
		};

		_dataFlowExecutionUserIdProvider.GetDataFlowExecutionUserId(expectedExecutionId).Returns(userId);
		_tableQueryCommandInvoker.InvokeQueryCommandAsync(expectedCommand, CancellationToken.None)
			.Returns(Task.FromResult(expectedReport));

		// Act
		Func<Task<TableQueryResponseDto>> act = async () => await _sut.QueryTableAsync(
			dataFlowExecutionId,
			nodeId,
			outputPortId,
			includeSchema,
			queryParameters,
			CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<TableQueryExecutionFailedException>();
	}

	[Fact]
	public async Task QueryTableAsync_ShouldThrowNodeExecutionStateNotSucceededException_WhenNodeIsNotSucceeded()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var outputPortId = Guid.NewGuid();
		const bool includeSchema = true;
		ReadOnlyDictionary<string, string> queryParameters = new Dictionary<string, string>().AsReadOnly();

		var expectedExecutionId = new DataFlowExecutionId(dataFlowExecutionId);
		var expectedNodeId = new NodeId(nodeId);

		_nodeExecutionStateValidator
			.When(v => v.ValidateNodeIsSucceeded(expectedExecutionId, expectedNodeId))
			.Do(_ => throw new NodeExecutionStateNotSucceededException());

		// Act
		Func<Task> act = async () => await _sut.QueryTableAsync(
			dataFlowExecutionId,
			nodeId,
			outputPortId,
			includeSchema,
			queryParameters,
			CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NodeExecutionStateNotSucceededException>();
	}

	[Fact]
	public async Task GetRowCountAsync_ShouldReturnRowCountFromReport_WhenAllOperationsSucceed()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var outputPortId = Guid.NewGuid();
		var queryParameters = new Dictionary<string, string> { { "filter", "value" } };
		const long expectedRowCount = 42L;

		var userId = new UserId(Guid.NewGuid());
		var expectedExecutionId = new DataFlowExecutionId(dataFlowExecutionId);
		var expectedCommand = new CalculateRowCountCommand
		{
			GraphExecutionId = dataFlowExecutionId,
			UserId = userId.Value,
			NodeId = nodeId,
			OutputPortId = outputPortId,
			ODataQuery = queryParameters
		};
		var expectedReport = new CalculateRowCountReport
		{
			GraphExecutionId = dataFlowExecutionId,
			NodeId = nodeId,
			PortId = outputPortId,
			RowCount = expectedRowCount,
			IsSucceeded = true
		};

		_dataFlowExecutionUserIdProvider.GetDataFlowExecutionUserId(expectedExecutionId).Returns(userId);
		_rowCountQueryCommandInvoker.InvokeQueryCommandAsync(expectedCommand, CancellationToken.None)
			.Returns(Task.FromResult(expectedReport));

		// Act
		var result = await _sut.GetRowCountAsync(
			dataFlowExecutionId,
			nodeId,
			outputPortId,
			queryParameters,
			CancellationToken.None);

		// Assert
		result.Should().Be(expectedRowCount);
	}

	[Fact]
	public async Task GetRowCountAsync_ShouldThrowNodeExecutionStateNotSucceededException_WhenNodeIsNotSucceeded()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var outputPortId = Guid.NewGuid();
		ReadOnlyDictionary<string, string> queryParameters = new Dictionary<string, string>().AsReadOnly();

		var expectedExecutionId = new DataFlowExecutionId(dataFlowExecutionId);
		var expectedNodeId = new NodeId(nodeId);

		_nodeExecutionStateValidator
			.When(v => v.ValidateNodeIsSucceeded(expectedExecutionId, expectedNodeId))
			.Do(_ => throw new NodeExecutionStateNotSucceededException());

		// Act
		Func<Task> act = async () => await _sut.GetRowCountAsync(
			dataFlowExecutionId,
			nodeId,
			outputPortId,
			queryParameters,
			CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NodeExecutionStateNotSucceededException>();
	}

	[Fact]
	public async Task GetRowCountAsync_ShouldThrowTableQueryExecutionFailedException_WhenRowCountIsMissing()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var outputPortId = Guid.NewGuid();
		var queryParameters = new Dictionary<string, string>();

		var userId = new UserId(Guid.NewGuid());
		var expectedExecutionId = new DataFlowExecutionId(dataFlowExecutionId);
		var expectedCommand = new CalculateRowCountCommand
		{
			GraphExecutionId = dataFlowExecutionId,
			UserId = userId.Value,
			NodeId = nodeId,
			OutputPortId = outputPortId,
			ODataQuery = queryParameters
		};
		var failedReport = new CalculateRowCountReport
		{
			GraphExecutionId = dataFlowExecutionId,
			NodeId = nodeId,
			PortId = outputPortId,
			RowCount = null,
			IsSucceeded = true
		};

		_dataFlowExecutionUserIdProvider.GetDataFlowExecutionUserId(expectedExecutionId).Returns(userId);
		_rowCountQueryCommandInvoker.InvokeQueryCommandAsync(expectedCommand, CancellationToken.None)
			.Returns(Task.FromResult(failedReport));

		// Act
		Func<Task> act = async () => await _sut.GetRowCountAsync(
			dataFlowExecutionId,
			nodeId,
			outputPortId,
			queryParameters,
			CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<TableQueryExecutionFailedException>();
	}
}
