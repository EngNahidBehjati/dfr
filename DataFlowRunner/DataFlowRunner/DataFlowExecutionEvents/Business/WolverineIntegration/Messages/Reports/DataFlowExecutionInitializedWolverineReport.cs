using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;
using Wolverine.Attributes;

namespace DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Reports;

[MessageIdentity("DataFlowExecutionInitializedReport")]
public sealed record DataFlowExecutionInitializedWolverineReport
{
	public required Guid GraphExecutionId { get; init; }
	public required bool IsSucceeded { get; init; }
	public GraphExecutionError? Error { get; init; }
}
