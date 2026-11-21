using DataFlowRunner.Common.Exceptions.NotFound;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Exceptions.Conflict;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;

namespace DataFlowRunner.GraphCore.Models;

internal sealed class Graph : IGraph
{
	private readonly Dictionary<NodeId, Node> _nodes = new();
	private readonly Dictionary<EdgeId, EdgeByNode> _edges = new();
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public Graph(ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_languageDictionaryProvider = languageDictionaryProvider
		                              ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public void AddNodes(IReadOnlyCollection<NodeId> nodes)
	{
		foreach (NodeId node in nodes)
		{
			if (!_nodes.TryAdd(node, new Node { Id = node }))
			{
				var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage
					.DataFlowRunner.GraphCore.DuplicateNode(node.Value);
				throw new DuplicateNodeException(errorMessage);
			}
		}
	}

	public void RemoveNodes(IReadOnlyCollection<NodeId> nodeIds)
	{
		foreach (NodeId nodeId in nodeIds)
		{
			_nodes.Remove(nodeId);
		}
	}

	public IReadOnlyCollection<Node> GetAllNodes()
	{
		return _nodes.Values.ToList();
	}

	public bool NodeExists(NodeId nodeId)
	{
		return _nodes.ContainsKey(nodeId);
	}

	public void AddEdges(IReadOnlyCollection<EdgeByNode> edges)
	{
		foreach (EdgeByNode edge in edges)
		{
			var edgeByNode = new EdgeByNode
			{
				Id = edge.Id,
				FromNodeId = edge.FromNodeId,
				ToNodeId = edge.ToNodeId
			};
			if (!_edges.TryAdd(edge.Id, edgeByNode))
			{
				var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage
					.DataFlowRunner.GraphCore.DuplicateEdge(edge.Id.Value);
				throw new DuplicateEdgeException(errorMessage);
			}
		}
	}

	public void RemoveEdges(IReadOnlyCollection<EdgeId> edges)
	{
		foreach (EdgeId edge in edges)
		{
			_edges.Remove(edge);
		}
	}

	public IReadOnlyCollection<EdgeByNode> GetEdges(IReadOnlyCollection<EdgeId> edges)
	{
		return edges.Select(edgeId =>
			{
				if (_edges.TryGetValue(edgeId, out EdgeByNode? edge))
				{
					return edge;
				}
				var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage
					.DataFlowRunner.GraphCore.EdgeNotFound();
				throw new EdgeNotFoundException(errorMessage);
			})
			.ToList();
	}

	public IReadOnlyCollection<EdgeByNode> GetAllEdges()
	{
		return _edges.Values.ToList();
	}
}
