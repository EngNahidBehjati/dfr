using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowCleanup.Abstraction;

internal interface INodeRemover
{
	Task RemoveNodesAsync(DataFlowExecutionId dataFlowExecutionId, List<NodeId> nodeIds, CancellationToken cancellationToken);
}
