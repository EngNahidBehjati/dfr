using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;

namespace DataFlowRunner.GraphTraversal.Business;

internal sealed class ChildNodesFinder : IChildNodesFinder
{
	public IReadOnlySet<NodeId> GetChildNodeIds(NodeId nodeId, IReadOnlyGraph graph)
	{
		ArgumentNullException.ThrowIfNull(graph);

		return graph.GetAllEdges()
			.Where(edge => edge.FromNodeId == nodeId)
			.Select(edge => edge.ToNodeId)
			.ToHashSet();
	}
}
