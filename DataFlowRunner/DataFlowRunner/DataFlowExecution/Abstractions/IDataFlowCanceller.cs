using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

internal interface IDataFlowCanceller
{
	Task CancelDataFlowAsync(DataFlowExecutionContext dataFlowExecutionContext, CancellationToken cancellationToken);

	Task CancelNodeAndDownstreamAsync(DataFlowExecutionContext dataFlowExecutionContext, NodeId sourceNodeId,
		CancellationToken cancellationToken);
}
