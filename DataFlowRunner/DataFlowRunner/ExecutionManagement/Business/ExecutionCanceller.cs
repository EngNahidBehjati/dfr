using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.ExecutionManagement.Abstractions;
using DataFlowRunner.ExecutionManagement.Models;

namespace DataFlowRunner.ExecutionManagement.Business;

internal sealed class ExecutionCanceller : IExecutionCanceller
{
	private readonly ICancelExecutionCommandPublisher _cancelExecutionCommandPublisher;
	private readonly ILogger<ExecutionCanceller> _logger;
	private readonly IDataFlowExecutionFacade _dataFlowExecutionFacade;

	public ExecutionCanceller(ICancelExecutionCommandPublisher cancelExecutionCommandPublisher,
		ILogger<ExecutionCanceller> logger,
		IDataFlowExecutionFacade dataFlowExecutionFacade)
	{
		_cancelExecutionCommandPublisher = cancelExecutionCommandPublisher ?? throw new ArgumentNullException(nameof(cancelExecutionCommandPublisher));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_dataFlowExecutionFacade = dataFlowExecutionFacade ?? throw new ArgumentNullException(nameof(dataFlowExecutionFacade));
	}

	public async Task CancelExecutionAsync(CancelCommandDto commandDto,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation(
			"Cancellation requested from source {ExecutionSource} for dataFlow with id {DataFlowId} and execution id {ExecutionId}",
			commandDto.ExecutionSource, commandDto.DataFlowId, commandDto.DataFlowExecutionId);

		if (commandDto.ExecutionSource == "WEB_DATA_FLOW_PAGE")
		{
			await _dataFlowExecutionFacade.CancelDataFlow(commandDto.DataFlowExecutionId, cancellationToken)
				.ConfigureAwait(false);
		}
		else
		{
			await _cancelExecutionCommandPublisher.PublishCommandAsync(
				commandDto, cancellationToken).ConfigureAwait(false);
		}
	}
}
