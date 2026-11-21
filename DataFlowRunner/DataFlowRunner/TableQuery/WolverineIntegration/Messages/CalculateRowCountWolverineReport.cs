using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;
using Wolverine.Attributes;

namespace DataFlowRunner.TableQuery.WolverineIntegration.Messages;

[MessageIdentity("CalculateRowCountReport")]
internal sealed record CalculateRowCountWolverineReport
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid NodeId { get; init; }
	public required Guid PortId { get; init; }
	public required long? RowCount { get; init; }
	public required bool IsSucceeded { get; init; }
	public GraphExecutionError? Error { get; init; }
}
