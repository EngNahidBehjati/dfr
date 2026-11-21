namespace DataFlowRunner.NodeExecutionStates.DataTransferObjects;

public sealed class DataFlowExecutionStatusDto
{
	public required ExecutionStatusDto ExecutionStatus { get; init; }
	public required List<NodeExecutionStateByIdDto> NodeExecutionStates { get; init; }
}
