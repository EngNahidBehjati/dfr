namespace DataFlowRunner.NodeExecutionStates.DataTransferObjects;

public sealed record NodeErrorMessageDto
{
	public required string ErrorMessage { get; init; }
	public required string? StackTrace { get; init; }
}
