using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;

internal interface IChildNodesExecutor
{
	Task RunReadyChildNodes(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId,
		CancellationToken cancellationToken);
}
