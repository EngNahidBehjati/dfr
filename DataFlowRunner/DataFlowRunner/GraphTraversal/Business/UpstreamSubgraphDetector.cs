using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;

namespace DataFlowRunner.GraphTraversal.Business;

internal sealed class UpstreamSubgraphDetector : IUpstreamSubgraphDetector
{
	private readonly ISubGraphCreator _subGraphCreator;

	public UpstreamSubgraphDetector(ISubGraphCreator subGraphCreator)
	{
		_subGraphCreator = subGraphCreator ?? throw new ArgumentNullException(nameof(subGraphCreator));
	}

	public IReadOnlyGraph DetectUpstreamSubgraph(IReadOnlyGraph graph, DataFlowExecutionId dataFlowExecutionId, NodeId nodeId)
	{
		HashSet<NodeId> subgraphNodeIds = CollectAncestralNodes(nodeId, graph.GetAllEdges());
		return _subGraphCreator.CreateSubGraphFromNodes(graph, subgraphNodeIds);
	}

	private HashSet<NodeId> CollectAncestralNodes(NodeId targetNodeId,
		IReadOnlyCollection<EdgeByNode> edges)
	{
		var nodes = new HashSet<NodeId> { targetNodeId };
		var nodesToGetParents = new Stack<NodeId>();
		nodesToGetParents.Push(targetNodeId);

		var edgesByInputNode = edges
			.GroupBy(edge => edge.ToNodeId)
			.ToDictionary(group => group.Key, group => group.ToList());

		while (nodesToGetParents.Count > 0)
		{
			NodeId currentNode = nodesToGetParents.Pop();

			if (!edgesByInputNode.TryGetValue(currentNode, out List<EdgeByNode>? incomingEdges))
			{
				continue;
			}

			IEnumerable<NodeId> parentNodeIds = incomingEdges.Select(edge => edge.FromNodeId);

			foreach (NodeId nodeId in parentNodeIds)
			{
				if (nodes.Add(nodeId))
				{
					nodesToGetParents.Push(nodeId);
				}
			}
		}

		return nodes;
	}
}
