namespace DataFlowRunner.NodeExecutionStates.Models;

internal sealed record NodeErrorMessage
{
	public required string Message { get; init; }
	public required string? StackTrace { get; init; }
}
