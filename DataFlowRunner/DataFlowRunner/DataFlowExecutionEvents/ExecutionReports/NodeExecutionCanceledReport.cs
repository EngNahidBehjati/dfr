namespace DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;

public sealed record NodeExecutionCanceledReport
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid NodeId { get; init; }
	public required bool IsSucceeded { get; init; }
	public GraphExecutionError? Error { get; init; }
}
