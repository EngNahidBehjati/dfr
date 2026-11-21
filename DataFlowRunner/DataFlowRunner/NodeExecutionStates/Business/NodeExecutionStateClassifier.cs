using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Business;

internal sealed class NodeExecutionStateClassifier : INodeExecutionStateClassifier
{
	public bool IsExecuting(NodeExecutionState state)
	{
		return state switch
		{
			NodeExecutionState.Canceling or
				NodeExecutionState.Pending or
				NodeExecutionState.Running => true,
			NodeExecutionState.Idle or
				NodeExecutionState.Succeeded or
				NodeExecutionState.Failed or
				NodeExecutionState.Canceled => false,
			_ => throw new ArgumentOutOfRangeException(nameof(state))
		};
	}

	public bool IsRunnable(NodeExecutionState state)
	{
		return state switch
		{
			NodeExecutionState.Canceling or
				NodeExecutionState.Pending or
				NodeExecutionState.Running or
				NodeExecutionState.Succeeded => false,
			NodeExecutionState.Idle or
				NodeExecutionState.Failed
				or NodeExecutionState.Canceled => true,
			_ => throw new ArgumentOutOfRangeException(nameof(state))
		};
	}
}
