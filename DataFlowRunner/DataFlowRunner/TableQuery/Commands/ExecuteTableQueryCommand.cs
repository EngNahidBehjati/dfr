using System.Text.Json;
using DataFlowRunner.TableQuery.Abstractions;

namespace DataFlowRunner.TableQuery.Commands;

public sealed record ExecuteTableQueryCommand : IQueryCommand
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid UserId { get; init; }
	public required Guid NodeId { get; init; }
	public required Guid OutputPortId { get; init; }
	public required bool IncludeSchema { get; init; }
	public required IReadOnlyDictionary<string, string> ODataQuery { get; init; }

	public IReadOnlyDictionary<string, object> GetLogParameters()
	{
		return new Dictionary<string, object>()
		{
			["GraphExecutionId"] = GraphExecutionId,
			["UserId"] = UserId,
			["NodeId"] = NodeId,
			["OutputPortId"] = OutputPortId,
			["IncludeSchema"] = IncludeSchema,
			["ODataQuery"] = JsonSerializer.Serialize(ODataQuery),
			["CommandType"] = nameof(ExecuteTableQueryCommand)
		};
	}
}
