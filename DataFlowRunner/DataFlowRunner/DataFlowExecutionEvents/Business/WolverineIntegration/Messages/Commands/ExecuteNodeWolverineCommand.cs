using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using Wolverine.Attributes;

namespace DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Commands;

[MessageIdentity("ExecuteNodeCommand")]
internal sealed record ExecuteNodeWolverineCommand : IWolverineExecutionCommand
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid UserId { get; init; }
	public required Guid NodeId { get; init; }
	public required string Config { get; init; }
	public required IReadOnlyCollection<NodeInputPortToUpstreamPorts>? PortConnections { get; init; }
	public required IReadOnlyCollection<ExecutionDynamicParameter> DynamicParameters { get; init; }
}
