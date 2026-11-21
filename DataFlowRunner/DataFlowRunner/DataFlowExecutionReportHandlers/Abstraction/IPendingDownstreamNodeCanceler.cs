using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;

internal interface IPendingDownstreamNodeCanceler
{
	Task CancelAsync(DataFlowExecutionId executionId, NodeId sourceNodeId, CancellationToken cancellationToken);
}
