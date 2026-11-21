using System.Diagnostics.CodeAnalysis;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Abstractions;

internal interface INodeExecutionStateProvider
{
	IReadOnlyDictionary<NodeId, NodeExecutionState> GetAllNodeExecutionStates(DataFlowExecutionId dataFlowExecutionId);
	NodeExecutionState GetNodeExecutionState(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId);
	bool TryGetNodeExecutionState(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId,
		[NotNullWhen(true)] out NodeExecutionState? state);
}
