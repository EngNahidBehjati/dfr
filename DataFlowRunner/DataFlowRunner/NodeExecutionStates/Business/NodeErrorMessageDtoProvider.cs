using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.DataTransferObjects;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Business;

internal sealed class NodeErrorMessageDtoProvider : INodeErrorMessageDtoProvider
{
	private readonly INodeErrorMessageMapper _nodeErrorMessageMapper;
	private readonly INodeErrorMessageStore _nodeErrorMessageStore;

	public NodeErrorMessageDtoProvider(INodeErrorMessageMapper nodeErrorMessageMapper,
		INodeErrorMessageStore nodeErrorMessageStore)
	{
		_nodeErrorMessageMapper = nodeErrorMessageMapper ?? throw new ArgumentNullException(nameof(nodeErrorMessageMapper));
		_nodeErrorMessageStore = nodeErrorMessageStore ?? throw new ArgumentNullException(nameof(nodeErrorMessageStore));
	}

	public Task<NodeErrorMessageDto> GetNodeErrorMessageAsync(Guid dataFlowExecutionId,
		Guid nodeId,
		CancellationToken cancellationToken)
	{
		NodeErrorMessage message = _nodeErrorMessageStore.GetNodeErrorMessage(dataFlowExecutionId, nodeId);
		NodeErrorMessageDto dto = _nodeErrorMessageMapper.Map(message);
		return Task.FromResult(dto);
	}
}
