using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Abstractions;

internal interface INodeExecutionStateUpdater
{
	void ChangeNodeExecutionState(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId, NodeExecutionState state);
	void TryAddNodeExecutionState(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId);
	void ResetNodeExecutionState(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId);
}

