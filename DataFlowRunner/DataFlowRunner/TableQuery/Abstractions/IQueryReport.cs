using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;

namespace DataFlowRunner.TableQuery.Abstractions;

public interface IQueryReport
{
	Guid GraphExecutionId { get; init; }
	Guid NodeId { get; init; }
	Guid PortId { get; init; }
	bool IsSucceeded { get; init; }
	GraphExecutionError? Error { get; init; }
}
