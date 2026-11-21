using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class DataFlowCancellationNodeNextState : IDataFlowCancellationNodeNextState
{
	private static readonly Dictionary<NodeExecutionState, NodeExecutionState> _nextNodeState =
		new()
		{
			[NodeExecutionState.Pending] = NodeExecutionState.Idle,
			[NodeExecutionState.Running] = NodeExecutionState.Canceling
		};

	public bool TryGetNodeNextState(NodeExecutionState currentNodeExecutionState, out NodeExecutionState nextNodeState)
	{
		return _nextNodeState.TryGetValue(currentNodeExecutionState, out nextNodeState);
	}
}
