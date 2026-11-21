using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;
using Wolverine.Attributes;

namespace DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Reports;

[MessageIdentity("NodeExecutionCanceledReport")]
public sealed record NodeExecutionCanceledWolverineReport
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid NodeId { get; init; }
	public required bool IsSucceeded { get; init; }
	public GraphExecutionError? Error { get; init; }
}
