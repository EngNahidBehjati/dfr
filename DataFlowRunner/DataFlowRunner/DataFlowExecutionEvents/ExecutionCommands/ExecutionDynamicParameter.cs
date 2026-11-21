namespace DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;

internal sealed record ExecutionDynamicParameter
{
	public required string Name { get; init; }
	public required object? Value { get; init; }
}
