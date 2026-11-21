using DataFlowRunner.DataFlowExecutionEvents.Abstractions;

namespace DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;

internal sealed record DisposeDataFlowExecutionCommand : IExecutionCommand
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid UserId { get; init; }

	public IReadOnlyDictionary<string, object> GetLogParameters()
	{
		return new Dictionary<string, object>()
		{
			["GraphExecutionId"] = GraphExecutionId,
			["UserId"] = UserId,
			["CommandType"] = nameof(DisposeDataFlowExecutionCommand)
		};
	}
}
