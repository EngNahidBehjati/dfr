namespace DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;

public sealed record DataFlowExecutionInitializedReport
{
	public required Guid GraphExecutionId { get; init; }
	public required bool IsSucceeded { get; init; }
	public GraphExecutionError? Error { get; init; }
}
