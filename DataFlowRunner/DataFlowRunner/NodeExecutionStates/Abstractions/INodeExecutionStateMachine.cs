using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Abstractions;

internal interface INodeExecutionStateMachine
{
	bool CanTransition(NodeExecutionState currentState, NodeExecutionState nextState);
}
