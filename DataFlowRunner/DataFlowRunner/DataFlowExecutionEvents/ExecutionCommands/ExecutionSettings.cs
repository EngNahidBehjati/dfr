namespace DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;

internal sealed record ExecutionSettings
{
	public required string ExecutorType { get; init; }
	public Guid? ExecutorId { get; init; }
}
