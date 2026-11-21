namespace DataFlowRunner.TableQuery.WolverineIntegration.Abstractions;

internal interface IWolverineQueryCommand
{
	Guid GraphExecutionId { get; }
}
