using System.Text.Json.Serialization;

namespace DataFlowRunner.NodeExecutionStates.DataTransferObjects;

public sealed record NodeExecutionStateByIdDto
{
	public Guid NodeId { get; init; }

	[JsonPropertyName("executionState")]
	public NodeExecutionStateDto NodeExecutionStateDto { get; init; }
	public long? DurationMilliseconds { get; init; }
}
