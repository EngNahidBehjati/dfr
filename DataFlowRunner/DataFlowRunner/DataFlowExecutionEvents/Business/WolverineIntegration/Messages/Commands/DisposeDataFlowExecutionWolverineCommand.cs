using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Abstractions;
using Wolverine.Attributes;

namespace DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Commands;

[MessageIdentity("DisposeDataFlowExecutionCommand")]
internal sealed record DisposeDataFlowExecutionWolverineCommand : IWolverineExecutionCommand
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid UserId { get; init; }
}
