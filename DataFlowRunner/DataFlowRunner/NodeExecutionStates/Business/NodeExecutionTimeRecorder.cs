using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Business;

internal sealed class NodeExecutionTimeRecorder : INodeExecutionTimeRecorder
{
	private readonly INodeExecutionTimingStore _nodeExecutionTimingStore;
	private readonly INodeExecutionStateClassifier _nodeExecutionStateClassifier;

	public NodeExecutionTimeRecorder(INodeExecutionTimingStore nodeExecutionTimingStore,
		INodeExecutionStateClassifier nodeExecutionStateClassifier)
	{
		_nodeExecutionTimingStore =
			nodeExecutionTimingStore ?? throw new ArgumentNullException(nameof(nodeExecutionTimingStore));
		_nodeExecutionStateClassifier = nodeExecutionStateClassifier ??
		                                throw new ArgumentNullException(nameof(nodeExecutionStateClassifier));
	}

	public void RecordNodeExecutionTime(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId,
		NodeExecutionState lastState, NodeExecutionState nextState)
	{
		if (!_nodeExecutionStateClassifier.IsExecuting(lastState) &&
		    _nodeExecutionStateClassifier.IsExecuting(nextState))
		{
			_nodeExecutionTimingStore.SetNodeExecutionStartTime(dataFlowExecutionId, nodeId);
		}
		else if (_nodeExecutionStateClassifier.IsExecuting(lastState) &&
		         !_nodeExecutionStateClassifier.IsExecuting(nextState))
		{
			_nodeExecutionTimingStore.SetNodeExecutionEndTime(dataFlowExecutionId, nodeId);
		}
	}
}
