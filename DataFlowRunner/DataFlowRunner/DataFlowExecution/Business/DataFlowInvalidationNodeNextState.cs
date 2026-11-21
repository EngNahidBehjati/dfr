using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class DataFlowInvalidationNodeNextState : IDataFlowInvalidationNodeNextState
{
	private static readonly Dictionary<NodeExecutionState, NodeExecutionState> _nextNodeState =
		new()
		{
			[NodeExecutionState.Pending] = NodeExecutionState.Idle,
			[NodeExecutionState.Running] = NodeExecutionState.Canceling,
			[NodeExecutionState.Succeeded] = NodeExecutionState.Idle
		};

	public bool TryGetNodeNextState(NodeExecutionState currentNodeExecutionState, out NodeExecutionState nextNodeState)
	{
		return _nextNodeState.TryGetValue(currentNodeExecutionState, out nextNodeState);
	}
}
