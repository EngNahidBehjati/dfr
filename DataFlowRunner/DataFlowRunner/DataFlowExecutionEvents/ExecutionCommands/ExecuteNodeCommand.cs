using DataFlowRunner.DataFlowExecutionEvents.Abstractions;

namespace DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;

internal sealed record ExecuteNodeCommand : IExecutionCommand
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid UserId { get; init; }
	public required Guid NodeId { get; init; }
	public required string Config { get; init; }
	public required IReadOnlyCollection<NodeInputPortToUpstreamPorts>? PortConnections { get; init; }
	public required IReadOnlyCollection<ExecutionDynamicParameter> DynamicParameters { get; init; }

	public IReadOnlyDictionary<string, object> GetLogParameters()
	{
		return new Dictionary<string, object>()
		{
			["GraphExecutionId"] = GraphExecutionId,
			["UserId"] = UserId,
			["NodeId"] = NodeId,
			["CommandType"] = nameof(ExecuteNodeCommand)
		};
	}
}
