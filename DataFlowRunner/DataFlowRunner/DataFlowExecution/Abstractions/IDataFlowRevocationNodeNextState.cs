using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

internal interface IDataFlowRevocationNodeNextState
{
	bool TryGetNodeNextState(NodeExecutionState currentNodeExecutionState, out NodeExecutionState nextNodeState);
}
