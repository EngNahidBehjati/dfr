namespace DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;

public sealed record GraphInitializationNotRequestedReport
{
	public required Guid GraphExecutionId { get; init; }
}
