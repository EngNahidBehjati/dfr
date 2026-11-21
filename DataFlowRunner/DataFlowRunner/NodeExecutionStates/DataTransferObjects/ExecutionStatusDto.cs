namespace DataFlowRunner.NodeExecutionStates.DataTransferObjects;

public sealed record ExecutionStatusDto
{
	public required bool Failed { get; init; }
	public required string? Message { get; init; }
}
