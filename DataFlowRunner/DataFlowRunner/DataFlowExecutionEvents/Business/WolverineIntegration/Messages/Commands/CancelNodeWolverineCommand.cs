using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Abstractions;
using Wolverine.Attributes;

namespace DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Commands;

[MessageIdentity("CancelNodeCommand")]
internal sealed record CancelNodeWolverineCommand : IWolverineExecutionCommand
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid UserId { get; init; }
	public required Guid NodeId { get; init; }
}
