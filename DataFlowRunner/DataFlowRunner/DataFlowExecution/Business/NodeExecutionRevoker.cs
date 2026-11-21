using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class NodeExecutionRevoker : INodeExecutionRevoker
{
	private readonly INodeExecutionStateUpdater _nodeExecutionStateUpdater;
	private readonly INodeExecutionStateProvider _nodeExecutionStateProvider;
	private readonly INodeCanceller _nodeCanceller;

	public NodeExecutionRevoker(INodeExecutionStateUpdater nodeExecutionStateUpdater,
		INodeExecutionStateProvider nodeExecutionStateProvider, INodeCanceller nodeCanceller)
	{
		_nodeExecutionStateUpdater = nodeExecutionStateUpdater ??
		                             throw new ArgumentNullException(nameof(nodeExecutionStateUpdater));
		_nodeExecutionStateProvider = nodeExecutionStateProvider ??
		                              throw new ArgumentNullException(nameof(nodeExecutionStateProvider));
		_nodeCanceller = nodeCanceller ?? throw new ArgumentNullException(nameof(nodeCanceller));
	}

	public async Task RevokeNodeExecution(DataFlowExecutionId dataFlowExecutionId,
		IReadOnlyCollection<NodeId> nodeIds,
		IDataFlowRevocationNodeNextState dataFlowRevocationNodeNextState,
		CancellationToken cancellationToken)
	{
		foreach (NodeId nodeId in nodeIds)
		{
			NodeExecutionState currentState =
				_nodeExecutionStateProvider.GetNodeExecutionState(dataFlowExecutionId, nodeId);
			if (currentState == NodeExecutionState.Running)
			{
				await _nodeCanceller
					.CancelNodeAsync(dataFlowExecutionId, nodeId, cancellationToken).ConfigureAwait(false);
			}

			if (dataFlowRevocationNodeNextState.TryGetNodeNextState(currentState,
				    out NodeExecutionState nextExecutionState))
			{
				_nodeExecutionStateUpdater.ChangeNodeExecutionState(dataFlowExecutionId, nodeId, nextExecutionState);
			}
		}
	}
}
