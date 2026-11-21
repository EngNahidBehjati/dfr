using DataFlowRunner.DataFlowExecutionEvents.Abstractions;

namespace DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;

internal sealed record CancelNodeCommand : IExecutionCommand
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid UserId { get; init; }
	public required Guid NodeId { get; init; }

	public IReadOnlyDictionary<string, object> GetLogParameters()
	{
		return new Dictionary<string, object>()
		{
			["GraphExecutionId"] = GraphExecutionId,
			["UserId"] = UserId,
			["NodeId"] = NodeId,
			["CommandType"] = nameof(CancelNodeCommand)
		};
	}
}
