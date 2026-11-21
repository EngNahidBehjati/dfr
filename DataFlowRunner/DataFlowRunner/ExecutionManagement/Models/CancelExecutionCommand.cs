using Wolverine.Attributes;

namespace DataFlowRunner.ExecutionManagement.Models;

[MessageIdentity("CancelExecutionCommand")]
internal sealed record CancelExecutionCommand
{
	public required string ExecutionSource { get; init; }

	public Guid DataFlowId { get; init; }

	public Guid DataFlowExecutionId { get; init; }
}
