namespace DataFlowRunner.ExecutionManagement.Models;

public sealed class RetryExecutionCommandDto
{
	public required Guid DataFlowId { get; init; }

	public required Guid DataFlowExecutionId { get; init; }

	public required string ExecutionSource { get; init; }

	public IReadOnlyCollection<RetryExecutionDynamicParameterDto> DynamicParameters { get; init; } = [];
}
