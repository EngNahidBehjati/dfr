using Wolverine.Attributes;

namespace DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Reports;

[MessageIdentity("GraphInitializationNotRequestedReport")]
public sealed record GraphInitializationNotRequestedWolverineReport
{
	public required Guid GraphExecutionId { get; init; }
}
