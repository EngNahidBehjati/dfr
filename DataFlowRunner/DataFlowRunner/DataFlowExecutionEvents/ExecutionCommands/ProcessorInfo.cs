namespace DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;

internal sealed record ProcessorInfo
{
	public required string ProcessorType { get; init; }
	public Guid? ProcessorId { get; init; }
}
