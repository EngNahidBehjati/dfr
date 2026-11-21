namespace DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;

public sealed record NodeExecutionSucceededReport
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid NodeId { get; init; }
}
