using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Abstractions;

internal interface INodeExecutionStateStore : INodeExecutionStateProvider
{
	void SetNodeExecutionState(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId, NodeExecutionState state);
	void RemoveNodeExecutionState(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId);
}
