namespace DataFlowRunner.ExecutionManagement.Models;

public sealed class CancelCommandDto
{
	public required string ExecutionSource { get; init; }

	public required Guid DataFlowId { get; init; }

	public required Guid DataFlowExecutionId { get; init; }
}
