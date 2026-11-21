using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;
using Wolverine.Attributes;

namespace DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Reports;

[MessageIdentity("NodeExecutionFailedReport")]
public sealed record NodeExecutionFailedWolverineReport
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid NodeId { get; init; }
	public required GraphExecutionError Error { get; init; }
}
