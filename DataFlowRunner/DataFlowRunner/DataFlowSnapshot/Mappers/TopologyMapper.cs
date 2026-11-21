using DataFlowRunner.DataFlowSnapshot.Abstractions;
using DataFlowRunner.DataFlowSnapshot.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowSnapshot.Mappers;

internal sealed class TopologyMapper : ITopologyMapper
{
	private readonly IGraphFactory _graphFactory;

	public TopologyMapper(IGraphFactory graphFactory)
	{
		_graphFactory = graphFactory ?? throw new ArgumentNullException(nameof(graphFactory));
	}

	public IGraph MapToGraph(Guid graphId, DataFlowTopologyDto dataFlowTopology)
	{
		IReadOnlyCollection<NodeId> nodes = GetNodes(dataFlowTopology.Topology.Nodes);
		IReadOnlyCollection<EdgeByNode> edges = GetEdges(dataFlowTopology.Topology.Edges);

		IGraph graph = _graphFactory.CreateGraph();

		graph.AddNodes(nodes);
		graph.AddEdges(edges);

		return graph;
	}

	private static List<EdgeByNode> GetEdges(
		IEnumerable<EdgeDto> edgeDtos)
	{
		return edgeDtos
			.Select(e => new EdgeByNode
			{
				Id = new EdgeId(e.EdgeId),
				FromNodeId = new NodeId(e.FromNode),
				ToNodeId = new NodeId(e.ToNode)
			})
			.ToList();
	}

	private static List<NodeId> GetNodes(IEnumerable<NodeDto> nodeDtos) =>
		nodeDtos.Select(x => new NodeId(x.NodeId)).ToList();
}
