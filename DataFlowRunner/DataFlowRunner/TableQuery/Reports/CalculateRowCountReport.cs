using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;
using DataFlowRunner.TableQuery.Abstractions;

namespace DataFlowRunner.TableQuery.Reports;

public sealed record CalculateRowCountReport : IQueryReport
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid NodeId { get; init; }
	public required Guid PortId { get; init; }
	public required long? RowCount { get; init; }
	public required bool IsSucceeded { get; init; }
	public GraphExecutionError? Error { get; init; }
}
