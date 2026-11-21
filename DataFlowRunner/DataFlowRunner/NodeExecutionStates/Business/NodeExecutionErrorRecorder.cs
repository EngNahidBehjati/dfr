using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Business;

internal sealed class NodeExecutionErrorRecorder : INodeExecutionErrorRecorder
{
	private readonly INodeExecutionStateUpdater _nodeExecutionStateUpdater;
	private readonly INodeErrorMessageStore _nodeErrorMessageStore;

	public NodeExecutionErrorRecorder(
		INodeExecutionStateUpdater nodeExecutionStateUpdater,
		INodeErrorMessageStore nodeErrorMessageStore)
	{
		_nodeExecutionStateUpdater =
			nodeExecutionStateUpdater ?? throw new ArgumentNullException(nameof(nodeExecutionStateUpdater));
		_nodeErrorMessageStore =
			nodeErrorMessageStore ?? throw new ArgumentNullException(nameof(nodeErrorMessageStore));
	}

	public void RecordFailure(DataFlowExecutionId dataFlowExecutionId,
		NodeId nodeId,
		NodeErrorMessage nodeErrorMessage)
	{
		Record(dataFlowExecutionId, nodeId, NodeExecutionState.Failed, nodeErrorMessage);
	}

	public void RecordCancellation(DataFlowExecutionId dataFlowExecutionId,
		NodeId nodeId,
		NodeErrorMessage nodeErrorMessage)
	{
		Record(dataFlowExecutionId, nodeId, NodeExecutionState.Canceled, nodeErrorMessage);
	}

	private void Record(DataFlowExecutionId dataFlowExecutionId,
		NodeId nodeId,
		NodeExecutionState nodeExecutionState,
		NodeErrorMessage nodeErrorMessage)
	{
		_nodeExecutionStateUpdater.ChangeNodeExecutionState(dataFlowExecutionId, nodeId, nodeExecutionState);
		_nodeErrorMessageStore.SetNodeErrorMessage(dataFlowExecutionId.Value, nodeId.Value, nodeErrorMessage);
	}
}
