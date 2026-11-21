namespace DataFlowRunner.DataFlowExecution.Dto;

public sealed class DynamicParameterExecutionDto
{
	public required string Name { get; init; }
	public required object? Value { get; init; }
}
