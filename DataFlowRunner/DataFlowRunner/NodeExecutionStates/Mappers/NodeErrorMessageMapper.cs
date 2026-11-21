using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.DataTransferObjects;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Mappers;

internal sealed class NodeErrorMessageMapper : INodeErrorMessageMapper
{
	public NodeErrorMessageDto Map(NodeErrorMessage nodeErrorMessage)
	{
		return new NodeErrorMessageDto
		{
			ErrorMessage = nodeErrorMessage.Message,
			StackTrace = nodeErrorMessage.StackTrace
		};
	}
}
