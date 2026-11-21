using DataFlowRunner.DataFlowExecution.Dto;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

public interface IDataFlowExecutionFacade
{
	Task RunDataFlowNode(Guid dataFlowExecutionId, Guid nodeId, long dataFlowVersion,
		RunDataFlowNodeDto runDataFlowNodeDto,
		CancellationToken cancellationToken);

	Task RunDataFlow(Guid dataFlowExecutionId, long dataFlowVersion,
		RunDataFlowDto runDataFlowDto,
		CancellationToken cancellationToken);

	Task CancelDataFlow(Guid dataFlowExecutionId, CancellationToken cancellationToken);

	Task CancelDataFlowNode(Guid dataFlowExecutionId, Guid nodeId, CancellationToken cancellationToken);

	Task InvalidateDataFlow(Guid dataFlowExecutionId, CancellationToken cancellationToken);

	Task InvalidateDataFlowNode(Guid dataFlowExecutionId, Guid nodeId, CancellationToken cancellationToken);
}
