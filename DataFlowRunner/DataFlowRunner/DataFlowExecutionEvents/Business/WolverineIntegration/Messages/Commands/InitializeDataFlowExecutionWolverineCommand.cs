using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using Wolverine.Attributes;

namespace DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Commands;

[MessageIdentity("InitializeDataFlowExecutionCommand")]
internal sealed record InitializeDataFlowExecutionWolverineCommand : IWolverineExecutionCommand
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid UserId { get; init; }
	public required Guid DataFlowId { get; init; }
	public required ProcessorInfo ProcessorInfo { get; init; }
}
