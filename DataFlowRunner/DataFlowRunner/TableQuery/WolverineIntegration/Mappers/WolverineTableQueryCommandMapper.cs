using DataFlowRunner.TableQuery.Abstractions;
using DataFlowRunner.TableQuery.Commands;
using DataFlowRunner.TableQuery.WolverineIntegration.Abstractions;
using DataFlowRunner.TableQuery.WolverineIntegration.Messages;
using Riok.Mapperly.Abstractions;

namespace DataFlowRunner.TableQuery.WolverineIntegration.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal sealed partial class WolverineTableQueryCommandMapper : IWolverineTableQueryCommandMapper
{
	public IWolverineQueryCommand ToWolverineCommand(IQueryCommand command)
	{
		ArgumentNullException.ThrowIfNull(command);

		return command switch
		{
			ExecuteTableQueryCommand executeCommand => MapExecuteCommand(executeCommand),
			CalculateRowCountCommand calculateCommand => MapCalculateCommand(calculateCommand),
			_ => throw new InvalidOperationException(
				$"Unsupported table query command type '{command.GetType().Name}'.")
		};
	}

	private partial ExecuteTableQueryWolverineCommand MapExecuteCommand(ExecuteTableQueryCommand command);

	private partial CalculateRowCountWolverineCommand MapCalculateCommand(CalculateRowCountCommand command);
}
