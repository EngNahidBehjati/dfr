using DataFlowRunner.DataFlowExecutionEvents.Abstractions;

namespace DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;

internal sealed record InitializeDataFlowExecutionCommand : IExecutionCommand
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid UserId { get; init; }
	public required Guid DataFlowId { get; init; }
	public required ProcessorInfo ProcessorInfo { get; init; }

	public IReadOnlyDictionary<string, object> GetLogParameters()
	{
		return new Dictionary<string, object>
		{
			["GraphExecutionId"] = GraphExecutionId,
			["UserId"] = UserId,
			["DataFlowId"] = DataFlowId,
			["CommandType"] = nameof(InitializeDataFlowExecutionCommand)
		};
	}
}
