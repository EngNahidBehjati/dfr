using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

internal interface INodeExecutor
{
	Task ExecuteNodesAsync(DataFlowExecutionId dataFlowExecutionId,
		IReadOnlySet<NodeId> nodeIds,
		CancellationToken cancellationToken);
}
