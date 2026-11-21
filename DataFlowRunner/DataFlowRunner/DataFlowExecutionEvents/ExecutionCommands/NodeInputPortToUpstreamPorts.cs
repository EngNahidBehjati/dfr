namespace DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;

internal sealed record NodeInputPortToUpstreamPorts
{
	public required Guid InputPort { get; init; }
	public required List<Guid> UpstreamPorts { get; init; }
}
