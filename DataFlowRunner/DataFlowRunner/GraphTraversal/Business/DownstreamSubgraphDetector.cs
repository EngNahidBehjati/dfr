using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.GraphTraversal.Business;

internal sealed class DownstreamSubgraphDetector : IDownstreamSubgraphDetector
{
	private readonly IChildNodesFinder _childNodesFinder;
	private readonly INodeExecutionStateProvider _nodeExecutionStateProvider;

	public DownstreamSubgraphDetector(IChildNodesFinder childNodesFinder,
		INodeExecutionStateProvider nodeExecutionStateProvider)
	{
		_childNodesFinder = childNodesFinder ?? throw new ArgumentNullException(nameof(childNodesFinder));
		_nodeExecutionStateProvider =
			nodeExecutionStateProvider ?? throw new ArgumentNullException(nameof(nodeExecutionStateProvider));
	}

	public IReadOnlySet<NodeId> DetectDownstreamNodes(
		IReadOnlyGraph graph,
		DataFlowExecutionId executionId,
		NodeId nodeId,
		Func<NodeExecutionState, bool> stopWhen)
	{
		return TraverseDownstreamNodes(
			graph,
			nodeId,
			candidateNodeId => stopWhen(_nodeExecutionStateProvider.GetNodeExecutionState(executionId, candidateNodeId)));
	}

	private IReadOnlySet<NodeId> TraverseDownstreamNodes(
		IReadOnlyGraph graph,
		NodeId nodeId,
		Func<NodeId, bool> stopCondition)
	{
		var visited = new HashSet<NodeId>();
		var queue = new Queue<NodeId>(_childNodesFinder.GetChildNodeIds(nodeId, graph));
		while (queue.Count > 0)
		{
			NodeId current = queue.Dequeue();

			if (visited.Contains(current) || stopCondition(current))
			{
				continue;
			}

			visited.Add(current);
			EnqueueDownstreamNodes(graph, current, queue, visited);
		}

		return visited;
	}

	private void EnqueueDownstreamNodes(
		IReadOnlyGraph graph,
		NodeId nodeId,
		Queue<NodeId> queue,
		HashSet<NodeId> visited)
	{
		IReadOnlySet<NodeId> childNodes = _childNodesFinder.GetChildNodeIds(nodeId, graph);

		foreach (NodeId childNode in childNodes)
		{
			if (!visited.Contains(childNode))
			{
				queue.Enqueue(childNode);
			}
		}
	}
}
