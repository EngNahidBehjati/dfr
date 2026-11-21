using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.GraphCore.Abstractions;

namespace DataFlowRunner.DataFlowExecution.Models;

internal sealed record DataFlowExecutionContext
{
	public required IGraph Graph { get; init; }
	public required DataFlowExecutionId DataFlowExecutionId { get; init; }
	public required DataFlowVersion DataFlowVersion { get; init; }
	public required ProcessorInfo ProcessorInfo { get; init; }
	public required ExecutionSettings ExecutionSettings { get; init; }
}
