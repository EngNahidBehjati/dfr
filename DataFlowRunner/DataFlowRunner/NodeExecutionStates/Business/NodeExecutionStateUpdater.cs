using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowSynchronization.Abstraction;
using DataFlowRunner.DataFlowSynchronization.Constants;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Business;

internal sealed class NodeExecutionStateUpdater : INodeExecutionStateUpdater
{
	private readonly INodeExecutionStateStore _nodeExecutionStateStore;
	private readonly INodeExecutionStateTransitionPolicy _nodeExecutionStateTransitionPolicy;
	private readonly INodeExecutionTimeRecorder _nodeExecutionTimeRecorder;
	private readonly IDataFlowActionLockExecutor _actionLockExecutor;

	public NodeExecutionStateUpdater(INodeExecutionStateStore nodeExecutionStateStore,
		INodeExecutionStateTransitionPolicy nodeExecutionStateTransitionPolicy,
		INodeExecutionTimeRecorder nodeExecutionTimeRecorder,
		IDataFlowActionLockExecutor actionLockExecutor)
	{
		_nodeExecutionStateStore = nodeExecutionStateStore ?? throw new ArgumentNullException(nameof(nodeExecutionStateStore));
		_nodeExecutionStateTransitionPolicy = nodeExecutionStateTransitionPolicy ?? throw new ArgumentNullException(nameof(nodeExecutionStateTransitionPolicy));
		_nodeExecutionTimeRecorder = nodeExecutionTimeRecorder ?? throw new ArgumentNullException(nameof(nodeExecutionTimeRecorder));
		_actionLockExecutor = actionLockExecutor ?? throw new ArgumentNullException(nameof(actionLockExecutor));
	}

	public void ChangeNodeExecutionState(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId,
		NodeExecutionState state)
	{
		_actionLockExecutor.Invoke(dataFlowExecutionId, LockActionKeys.UpdateNodeState, () =>
		{
			_nodeExecutionStateTransitionPolicy.EnsureTransitionAllowed(dataFlowExecutionId, nodeId, state);
			NodeExecutionState lastState = _nodeExecutionStateStore.GetNodeExecutionState(dataFlowExecutionId, nodeId);
			_nodeExecutionTimeRecorder.RecordNodeExecutionTime(dataFlowExecutionId, nodeId, lastState, state);
			_nodeExecutionStateStore.SetNodeExecutionState(dataFlowExecutionId, nodeId, state);
		});
	}

	public void TryAddNodeExecutionState(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId)
	{
		if (!_nodeExecutionStateStore.TryGetNodeExecutionState(dataFlowExecutionId, nodeId, out _))
			_nodeExecutionStateStore.SetNodeExecutionState(dataFlowExecutionId, nodeId, NodeExecutionState.Idle);
	}

	public void ResetNodeExecutionState(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId)
	{
		_actionLockExecutor.Invoke(dataFlowExecutionId, LockActionKeys.UpdateNodeState, () =>
		{
			NodeExecutionState lastState = _nodeExecutionStateStore.GetNodeExecutionState(dataFlowExecutionId, nodeId);
			_nodeExecutionTimeRecorder
				.RecordNodeExecutionTime(dataFlowExecutionId, nodeId, lastState, NodeExecutionState.Idle);
			_nodeExecutionStateStore.SetNodeExecutionState(dataFlowExecutionId, nodeId, NodeExecutionState.Idle);
		});
	}
}
