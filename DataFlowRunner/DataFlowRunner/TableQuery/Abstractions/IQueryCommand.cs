namespace DataFlowRunner.TableQuery.Abstractions;

public interface IQueryCommand
{
	Guid GraphExecutionId { get; init; }
	Guid UserId { get; init; }
	Guid NodeId { get; init; }
	Guid OutputPortId { get; init; }
	IReadOnlyDictionary<string, string> ODataQuery { get; init; }
	IReadOnlyDictionary<string, object> GetLogParameters();
}
