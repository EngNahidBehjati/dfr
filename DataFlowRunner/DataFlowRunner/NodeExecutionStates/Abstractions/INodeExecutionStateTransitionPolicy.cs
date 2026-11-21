using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Abstractions;

internal interface INodeExecutionStateTransitionPolicy
{
	void EnsureTransitionAllowed(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId, NodeExecutionState state);
}
