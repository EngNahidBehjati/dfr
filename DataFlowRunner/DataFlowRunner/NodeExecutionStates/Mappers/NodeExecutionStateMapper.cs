using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.DataTransferObjects;
using DataFlowRunner.NodeExecutionStates.Models;
using Riok.Mapperly.Abstractions;

namespace DataFlowRunner.NodeExecutionStates.Mappers;

[Mapper]
internal sealed partial class NodeExecutionStateMapper : INodeExecutionStateMapper
{
	public partial NodeExecutionStateDto NodeExecutionStateToDto(NodeExecutionState executionState);
}
