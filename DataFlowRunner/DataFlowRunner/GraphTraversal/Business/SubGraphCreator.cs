using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;

namespace DataFlowRunner.GraphTraversal.Business;

internal sealed class SubGraphCreator : ISubGraphCreator
{
	private readonly IGraphFactory _graphFactory;

	public SubGraphCreator(IGraphFactory graphFactory)
	{
		_graphFactory = graphFactory ?? throw new ArgumentNullException(nameof(graphFactory));
	}

	public IReadOnlyGraph CreateSubGraphFromNodes(IReadOnlyGraph source, IReadOnlyCollection<NodeId> subgraphNodeIds)
	{
		IGraph subgraph = _graphFactory.CreateGraph();
		subgraph.AddNodes(subgraphNodeIds);

		List<EdgeByNode> subgraphEdges = GetTargetEdges(source.GetAllEdges(), subgraphNodeIds);
		if (subgraphEdges.Count > 0)
		{
			subgraph.AddEdges(subgraphEdges);
		}

		return subgraph;
	}

	private static List<EdgeByNode> GetTargetEdges(IReadOnlyCollection<EdgeByNode> allEdges,
		IReadOnlyCollection<NodeId> subgraphNodeIds)
	{
		return allEdges
			.Where(edge => subgraphNodeIds.Contains(edge.ToNodeId) && subgraphNodeIds.Contains(edge.FromNodeId))
			.Select(edge => new EdgeByNode
			{
				Id = edge.Id,
				FromNodeId = edge.FromNodeId,
				ToNodeId = edge.ToNodeId
			})
			.ToList();
	}
}
