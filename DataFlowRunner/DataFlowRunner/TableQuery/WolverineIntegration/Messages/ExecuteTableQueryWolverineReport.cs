using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;
using DataFlowRunner.TableQuery.Reports;
using Wolverine.Attributes;

namespace DataFlowRunner.TableQuery.WolverineIntegration.Messages;

[MessageIdentity("ExecuteTableQueryReport")]
internal sealed record ExecuteTableQueryWolverineReport
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid NodeId { get; init; }
	public required Guid PortId { get; init; }
	public required IReadOnlyList<TableQueryReportColumn>? TableSchema { get; init; }
	public required IReadOnlyList<TableQueryReportRow>? Rows { get; init; }
	public required bool IsSucceeded { get; init; }
	public GraphExecutionError? Error { get; init; }
}
