using DataFlowRunner.NodeExecutionStates.DataTransferObjects;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Abstractions;

internal interface INodeErrorMessageMapper
{
	NodeErrorMessageDto Map(NodeErrorMessage nodeErrorMessage);
}
