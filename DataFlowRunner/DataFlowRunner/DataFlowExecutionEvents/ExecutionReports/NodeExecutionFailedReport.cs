namespace DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;

public sealed record NodeExecutionFailedReport
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid NodeId { get; init; }
	public required GraphExecutionError Error { get; init; }
}
