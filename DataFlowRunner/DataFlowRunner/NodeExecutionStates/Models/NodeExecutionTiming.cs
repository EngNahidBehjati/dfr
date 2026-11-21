namespace DataFlowRunner.NodeExecutionStates.Models;

internal sealed record NodeExecutionTiming
{
	public DateTimeOffset? StartedAt { get; init; }
	public DateTimeOffset? CompletedAt { get; set; }
}
