using Wolverine.Attributes;

namespace DataFlowRunner.ExecutionManagement.Models;

[MessageIdentity("RetryExecutionCommand")]
internal sealed record RetryExecutionCommand
{
	public required string ExecutionSource { get; init; }

	public required Guid DataFlowId { get; init; }

	public required Guid DataFlowExecutionId { get; init; }

	public required Guid UserId { get; init; }

	public required IReadOnlyCollection<RetryExecutionDynamicParameterDto> DynamicParameters { get; init; }
}
