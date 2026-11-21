using DataFlowRunner.DataFlowExecutionEvents.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Commands;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using Riok.Mapperly.Abstractions;

namespace DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal sealed partial class WolverineExecutionCommandMapper : IWolverineExecutionCommandMapper
{
	public IWolverineExecutionCommand ToWolverineCommand(IExecutionCommand command)
	{
		ArgumentNullException.ThrowIfNull(command);

		return command switch
		{
			InitializeDataFlowExecutionCommand initializeCommand => MapInitialize(initializeCommand),
			ExecuteNodeCommand executeCommand => MapExecuteNode(executeCommand),
			CancelNodeCommand cancelCommand => MapCancelNode(cancelCommand),
			DisposeDataFlowExecutionCommand disposeCommand => MapDispose(disposeCommand),
			ChangeDataFlowProcessorCommand changeCommand => MapChangeProcessor(changeCommand),
			_ => throw new InvalidOperationException(
				$"Unsupported execution command type '{command.GetType().Name}'.")
		};
	}

	private partial InitializeDataFlowExecutionWolverineCommand MapInitialize(
		InitializeDataFlowExecutionCommand command);

	private partial ExecuteNodeWolverineCommand MapExecuteNode(ExecuteNodeCommand command);

	private partial CancelNodeWolverineCommand MapCancelNode(CancelNodeCommand command);

	private partial DisposeDataFlowExecutionWolverineCommand MapDispose(DisposeDataFlowExecutionCommand command);

	private partial ChangeDataFlowProcessorWolverineCommand MapChangeProcessor(
		ChangeDataFlowProcessorCommand command);
}
