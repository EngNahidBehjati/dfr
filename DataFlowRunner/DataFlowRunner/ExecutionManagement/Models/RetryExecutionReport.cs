using Wolverine.Attributes;

namespace DataFlowRunner.ExecutionManagement.Models;

[MessageIdentity("RetryExecutionReport")]
internal sealed record RetryExecutionReport
{
	public required Guid DataFlowExecutionId { get; init; }

	public Guid? NewDataFlowExecutionId { get; init; }

	public required bool IsSucceeded { get; init; }

	public string? ErrorMessage { get; init; }
}
