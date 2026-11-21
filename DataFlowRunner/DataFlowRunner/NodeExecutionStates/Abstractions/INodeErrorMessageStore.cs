using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Abstractions;

internal interface INodeErrorMessageStore
{
	NodeErrorMessage GetNodeErrorMessage(Guid dataFlowExecutionId, Guid nodeId);
	void SetNodeErrorMessage(Guid dataFlowExecutionId, Guid nodeId, NodeErrorMessage message);
}
