using DataFlowRunner.DataFlowChangeEvents.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;

namespace DataFlowRunner.DataFlowChangeEvents.Business.Handlers;

internal sealed class ChangeExecutionSettingsHandler : IDataFlowChangeEventHandler<ChangeExecutionSettings>
{
	private readonly IDataFlowDisposer _dataFlowDisposer;

	public ChangeExecutionSettingsHandler(IDataFlowDisposer dataFlowDisposer)
	{
		_dataFlowDisposer = dataFlowDisposer ?? throw new ArgumentNullException(nameof(dataFlowDisposer));
	}

	public async Task HandleAsync(DataFlowChangeContext context, ChangeExecutionSettings @event,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(@event);

		await _dataFlowDisposer
			.DisposeAsync(context.DataFlowExecutionId, cancellationToken)
			.ConfigureAwait(false);

		context.ExecutionSettings = CreateExecutionSettings(@event.DataFlowExecutionSettings);
	}

	private static ExecutionSettings CreateExecutionSettings(DataFlowExecutionSettings dataFlowExecutionSettings)
	{
		return new ExecutionSettings
		{
			ExecutorType = dataFlowExecutionSettings.Type,
			ExecutorId = dataFlowExecutionSettings.ExecutorId
		};
	}
}
