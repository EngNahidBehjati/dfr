using DataFlowRunner.NodeExecutionStates.DataTransferObjects;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Abstractions;

internal interface INodeExecutionStateMapper
{
	NodeExecutionStateDto NodeExecutionStateToDto(NodeExecutionState executionState);
}
