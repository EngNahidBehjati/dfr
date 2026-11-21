using DataFlowRunner.NodeExecutionStates.DataTransferObjects;

namespace DataFlowRunner.NodeExecutionStates.Abstractions;

public interface INodeErrorMessageDtoProvider
{
	Task<NodeErrorMessageDto> GetNodeErrorMessageAsync(
		Guid dataFlowExecutionId, Guid nodeId, CancellationToken cancellationToken);
}
