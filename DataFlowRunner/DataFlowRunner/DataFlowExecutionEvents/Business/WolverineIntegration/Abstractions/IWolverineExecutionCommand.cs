namespace DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Abstractions;

internal interface IWolverineExecutionCommand
{
	Guid GraphExecutionId { get; }
}
