using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using Wolverine.Attributes;

namespace DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Commands;

[MessageIdentity("ChangeDataFlowProcessorCommand")]
internal sealed record ChangeDataFlowProcessorWolverineCommand : IWolverineExecutionCommand
{
	public required Guid GraphExecutionId { get; init; }
	public required Guid UserId { get; init; }
	public required ProcessorInfo ProcessorInfo { get; init; }
}
