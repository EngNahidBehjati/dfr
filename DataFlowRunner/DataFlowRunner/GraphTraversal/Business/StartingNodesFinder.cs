using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;

namespace DataFlowRunner.GraphTraversal.Business;

internal sealed class StartingNodesFinder : IStartingNodesFinder
{
	public IReadOnlySet<NodeId> FindStartingNodes(IReadOnlyGraph graph)
	{
		IReadOnlyCollection<EdgeByNode> edges = graph.GetAllEdges();
		var inputNodes = edges.Select(e => e.ToNodeId).ToHashSet();
		return graph.GetAllNodes().Select(n => n.Id).Except(inputNodes).ToHashSet();
	}
}
