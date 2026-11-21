using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.DataFlowExecutionReportHandlers.Business;

internal sealed class NodeExecutionReadinessChecker : INodeExecutionReadinessChecker
{
	private readonly INodeExecutionStateProvider _nodeExecutionStateProvider;

	public NodeExecutionReadinessChecker(INodeExecutionStateProvider nodeExecutionStateProvider)
	{
		_nodeExecutionStateProvider = nodeExecutionStateProvider ?? throw new ArgumentNullException(nameof(nodeExecutionStateProvider));
	}

	public bool IsNodeReadyToExecute(DataFlowExecutionContext dataFlowExecutionContext, NodeId nodeId)
	{
		ArgumentNullException.ThrowIfNull(dataFlowExecutionContext);
		if (!IsNodePending(dataFlowExecutionContext, nodeId))
		{
			return false;
		}

		IEnumerable<NodeId> parentNodes = GetParentNodes(dataFlowExecutionContext.Graph, nodeId);
		return AllNodeStatesSucceeded(dataFlowExecutionContext.DataFlowExecutionId, parentNodes);
	}

	private bool IsNodePending(DataFlowExecutionContext dataFlowExecutionContext, NodeId nodeId)
	{
		NodeExecutionState nodeState = _nodeExecutionStateProvider
			.GetNodeExecutionState(dataFlowExecutionContext.DataFlowExecutionId, nodeId);
		return nodeState == NodeExecutionState.Pending;
	}

	private bool AllNodeStatesSucceeded(DataFlowExecutionId dataFlowExecutionId, IEnumerable<NodeId> nodes)
	{
		return nodes.All(nodeId =>
		{
			NodeExecutionState nodeExecutionState = _nodeExecutionStateProvider
				.GetNodeExecutionState(dataFlowExecutionId, nodeId);
			return nodeExecutionState == NodeExecutionState.Succeeded;
		});
	}

	private static IEnumerable<NodeId> GetParentNodes(IGraph graph, NodeId nodeId)
	{
		return graph.GetAllEdges()
			.Where(edge => edge.ToNodeId == nodeId)
			.Select(edge => edge.FromNodeId);
	}
}
