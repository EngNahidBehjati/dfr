using Wolverine.Attributes;

namespace DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Reports;

[MessageIdentity("NodeExecutionSucceededReport")]
public sealed record NodeExecutionSucceededWolverineReport
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid NodeId { get; init; }
}
