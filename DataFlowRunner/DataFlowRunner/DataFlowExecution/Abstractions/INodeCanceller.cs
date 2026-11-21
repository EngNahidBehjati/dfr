using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

internal interface INodeCanceller
{
	Task CancelNodeAsync(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId, CancellationToken cancellationToken);
}
