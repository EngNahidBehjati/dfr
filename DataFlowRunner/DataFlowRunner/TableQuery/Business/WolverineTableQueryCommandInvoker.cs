using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.TableQuery.Abstractions;
using DataFlowRunner.TableQuery.Exceptions;
using DataFlowRunner.TableQuery.Options;
using DataFlowRunner.TableQuery.WolverineIntegration.Abstractions;
using Microsoft.Extensions.Options;
using Wolverine;

namespace DataFlowRunner.TableQuery.Business;

internal sealed class WolverineTableQueryCommandInvoker<TCommand, TReport, TWolverineReport>
	: ITableQueryCommandInvoker<TCommand, TReport>
	where TCommand : IQueryCommand
	where TReport : IQueryReport
	where TWolverineReport : class
{
	private readonly TimeSpan _invokeTimeout;
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<WolverineTableQueryCommandInvoker<TCommand, TReport, TWolverineReport>> _logger;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;
	private readonly IWolverineTableQueryCommandMapper _commandMapper;
	private readonly IWolverineTableQueryReportMapper<TWolverineReport, TReport> _reportMapper;

	public WolverineTableQueryCommandInvoker(IServiceScopeFactory serviceScopeFactory,
		IOptions<TableQueryInvokeTimeoutOptions> invokeTimeoutOptions,
		ILogger<WolverineTableQueryCommandInvoker<TCommand, TReport, TWolverineReport>> logger,
		ILanguageDictionaryProvider languageDictionaryProvider,
		IWolverineTableQueryCommandMapper commandMapper,
		IWolverineTableQueryReportMapper<TWolverineReport, TReport> reportMapper)
	{
		_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		ArgumentNullException.ThrowIfNull(invokeTimeoutOptions);
		_invokeTimeout = invokeTimeoutOptions.Value.TimeoutInSeconds;
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
		_commandMapper = commandMapper ?? throw new ArgumentNullException(nameof(commandMapper));
		_reportMapper = reportMapper ?? throw new ArgumentNullException(nameof(reportMapper));
	}

	public async Task<TReport> InvokeQueryCommandAsync(
		TCommand command,
		CancellationToken cancellationToken)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IMessageBus messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
		var deliveryOptions = new DeliveryOptions
		{
			PartitionKey = command.GraphExecutionId.ToString()
		};
		using IDisposable? disposable = _logger.BeginScope(command.GetLogParameters);
		TReport report;
		_logger.LogInformation("InvokeQueryCommandAsync started");
		try
		{
			report = await InvokeMappedQueryAsync(messageBus, command, deliveryOptions, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception e)
		{
			_logger.LogError(e, "InvokeQueryCommandAsync failed");
			throw;
		}

		_logger.LogInformation("InvokeQueryCommandAsync completed");
		EnsureReportSucceeded(report);
		return report;
	}

	private async Task<TReport> InvokeMappedQueryAsync(
		IMessageBus messageBus,
		TCommand command,
		DeliveryOptions deliveryOptions,
		CancellationToken cancellationToken)
	{
		IWolverineQueryCommand wolverineCommand = _commandMapper.ToWolverineCommand(command);
		TWolverineReport wolverineReport = await messageBus
			.InvokeAsync<TWolverineReport>(
				wolverineCommand, deliveryOptions, cancellationToken, _invokeTimeout)
			.ConfigureAwait(false);
		return _reportMapper.ToReport(wolverineReport);
	}

	private void EnsureReportSucceeded(TReport report)
	{
		if (report.IsSucceeded)
		{
			return;
		}

		var errorDetail = string.IsNullOrWhiteSpace(report.Error!.ErrorMessage)
			? _languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.Common.UnknownError()
			: report.Error!.ErrorMessage;

		var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage
			.DataFlowRunner.TableQuery.TableQueryExecutionFailed(errorDetail);
		throw new TableQueryExecutionFailedException(errorMessage, report.Error!.StackTrace);
	}
}
