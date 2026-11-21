using DataFlowRunner.TableQuery.WolverineIntegration.Abstractions;
using Wolverine.Attributes;

namespace DataFlowRunner.TableQuery.WolverineIntegration.Messages;

[MessageIdentity("ExecuteTableQueryCommand")]
internal sealed record ExecuteTableQueryWolverineCommand : IWolverineQueryCommand
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid UserId { get; init; }
	public required Guid NodeId { get; init; }
	public required Guid OutputPortId { get; init; }
	public required bool IncludeSchema { get; init; }
	public required IReadOnlyDictionary<string, string> ODataQuery { get; init; }
}
