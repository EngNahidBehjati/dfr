using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Abstractions;

internal interface INodeExecutionStateClassifier
{
	bool IsExecuting(NodeExecutionState state);
	bool IsRunnable(NodeExecutionState state);
}
