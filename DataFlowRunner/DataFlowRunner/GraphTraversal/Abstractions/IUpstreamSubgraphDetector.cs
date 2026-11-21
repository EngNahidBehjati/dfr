using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.GraphTraversal.Abstractions;

internal interface IUpstreamSubgraphDetector
{
	/// <summary>
	/// Returns a subgraph that contains the specified target node
	/// and all nodes that have a directed path to the target.
	/// </summary>
	IReadOnlyGraph DetectUpstreamSubgraph(IReadOnlyGraph graph, DataFlowExecutionId dataFlowExecutionId, NodeId nodeId);
}
