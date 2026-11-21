using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.GraphTraversal.Abstractions;

internal interface IDownstreamSubgraphDetector
{
	IReadOnlySet<NodeId> DetectDownstreamNodes(
		IReadOnlyGraph graph,
		DataFlowExecutionId executionId,
		NodeId nodeId,
		Func<NodeExecutionState, bool> stopWhen);
}
