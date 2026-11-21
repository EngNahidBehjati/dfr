using DataFlowRunner.TableQuery.Abstractions;

namespace DataFlowRunner.TableQuery.WolverineIntegration.Abstractions;

internal interface IWolverineTableQueryCommandMapper
{
	IWolverineQueryCommand ToWolverineCommand(IQueryCommand command);
}
