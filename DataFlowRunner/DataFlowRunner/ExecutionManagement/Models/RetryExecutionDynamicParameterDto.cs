namespace DataFlowRunner.ExecutionManagement.Models;

public sealed class RetryExecutionDynamicParameterDto
{
	public required string Name { get; init; }
	public required object? Value { get; init; }
}
