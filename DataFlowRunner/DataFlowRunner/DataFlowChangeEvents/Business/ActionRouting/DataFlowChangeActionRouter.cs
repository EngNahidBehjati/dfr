using System.Text.Json;
using DataFlowRunner.DataFlowChangeEvents.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;

namespace DataFlowRunner.DataFlowChangeEvents.Business.ActionRouting;

internal sealed class DataFlowChangeActionRouter : IDataFlowChangeActionRouter
{
	private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

	private readonly IDataFlowChangeEventHandler<AddNode> _addNodeHandler;
	private readonly IDataFlowChangeEventHandler<SetNodeConfig> _setNodeConfigHandler;
	private readonly IDataFlowChangeEventHandler<RemoveNode> _removeNodeHandler;
	private readonly IDataFlowChangeEventHandler<AddEdge> _addEdgeHandler;
	private readonly IDataFlowChangeEventHandler<RemoveEdge> _removeEdgeHandler;
	private readonly IDataFlowChangeEventHandler<CreateDataFlowVariable> _createDataFlowVariableHandler;
	private readonly IDataFlowChangeEventHandler<UpdateDataFlowVariable> _updateDataFlowVariableHandler;
	private readonly IDataFlowChangeEventHandler<DeleteDataFlowVariable> _deleteDataFlowVariableHandler;
	private readonly IDataFlowChangeEventHandler<ChangeProcessor> _changeProcessorHandler;
	private readonly IDataFlowChangeEventHandler<ChangeExecutionSettings> _changeExecutionSettingsHandler;
	private readonly ILogger<DataFlowChangeActionRouter> _logger;

	public DataFlowChangeActionRouter(
		IDataFlowChangeEventHandler<AddNode> addNodeHandler,
		IDataFlowChangeEventHandler<SetNodeConfig> setNodeConfigHandler,
		IDataFlowChangeEventHandler<RemoveNode> removeNodeHandler,
		IDataFlowChangeEventHandler<AddEdge> addEdgeHandler,
		IDataFlowChangeEventHandler<RemoveEdge> removeEdgeHandler,
		IDataFlowChangeEventHandler<CreateDataFlowVariable> createDataFlowVariableHandler,
		IDataFlowChangeEventHandler<UpdateDataFlowVariable> updateDataFlowVariableHandler,
		IDataFlowChangeEventHandler<DeleteDataFlowVariable> deleteDataFlowVariableHandler,
		IDataFlowChangeEventHandler<ChangeProcessor> changeProcessorHandler,
		IDataFlowChangeEventHandler<ChangeExecutionSettings> changeExecutionSettingsHandler,
		ILogger<DataFlowChangeActionRouter> logger)
	{
		_addNodeHandler = addNodeHandler ?? throw new ArgumentNullException(nameof(addNodeHandler));
		_setNodeConfigHandler = setNodeConfigHandler ?? throw new ArgumentNullException(nameof(setNodeConfigHandler));
		_removeNodeHandler = removeNodeHandler ?? throw new ArgumentNullException(nameof(removeNodeHandler));
		_addEdgeHandler = addEdgeHandler ?? throw new ArgumentNullException(nameof(addEdgeHandler));
		_removeEdgeHandler = removeEdgeHandler ?? throw new ArgumentNullException(nameof(removeEdgeHandler));
		_createDataFlowVariableHandler = createDataFlowVariableHandler ?? throw new ArgumentNullException(nameof(createDataFlowVariableHandler));
		_updateDataFlowVariableHandler = updateDataFlowVariableHandler ?? throw new ArgumentNullException(nameof(updateDataFlowVariableHandler));
		_deleteDataFlowVariableHandler = deleteDataFlowVariableHandler ?? throw new ArgumentNullException(nameof(deleteDataFlowVariableHandler));
		_changeProcessorHandler = changeProcessorHandler ?? throw new ArgumentNullException(nameof(changeProcessorHandler));
		_changeExecutionSettingsHandler = changeExecutionSettingsHandler ?? throw new ArgumentNullException(nameof(changeExecutionSettingsHandler));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public Task RouteAsync(string actionType,
		JsonElement action,
		DataFlowChangeContext context,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(actionType);
		ArgumentNullException.ThrowIfNull(context);

		return actionType switch
		{
			"ADD_PLUGIN_V1" => DeserializeAndHandleAsync(action, context, _addNodeHandler, cancellationToken),
			"SET_PLUGIN_CONFIG_V1" => DeserializeAndHandleAsync(action, context, _setNodeConfigHandler, cancellationToken),
			"REMOVE_PLUGIN_V1" => DeserializeAndHandleAsync(action, context, _removeNodeHandler, cancellationToken),
			"ADD_LINK_V1" => DeserializeAndHandleAsync(action, context, _addEdgeHandler, cancellationToken),
			"REMOVE_LINK_V1" => DeserializeAndHandleAsync(action, context, _removeEdgeHandler, cancellationToken),
			"CREATE_DATA_FLOW_VARIABLE_V1" => DeserializeAndHandleAsync(action, context, _createDataFlowVariableHandler, cancellationToken),
			"UPDATE_DATA_FLOW_VARIABLE_V1" => DeserializeAndHandleAsync(action, context, _updateDataFlowVariableHandler, cancellationToken),
			"DELETE_DATA_FLOW_VARIABLE_V1" => DeserializeAndHandleAsync(action, context, _deleteDataFlowVariableHandler, cancellationToken),
			"CHANGE_PIPELINE_DATA_DESTINATION_V1" => DeserializeAndHandleAsync(action, context, _changeProcessorHandler, cancellationToken),
			"CHANGE_PIPELINE_EXECUTION_SETTINGS_V1" => DeserializeAndHandleAsync(action, context, _changeExecutionSettingsHandler, cancellationToken),
			_ => UnknownAsync(actionType, context)
		};
	}

	private async Task DeserializeAndHandleAsync<TEvent>(
		JsonElement action,
		DataFlowChangeContext context,
		IDataFlowChangeEventHandler<TEvent> handler,
		CancellationToken cancellationToken)
	{
		TEvent? message = action.Deserialize<TEvent>(_serializerOptions);
		if (message is null)
		{
			_logger.LogWarning(
				"Failed to deserialize DataFlowChange action '{ActionType}'. DataFlowExecutionId={DataFlowExecutionId} DataFlowVersion={DataFlowVersion}",
				typeof(TEvent).Name,
				context.DataFlowExecutionId,
				context.NewVersion);
			return;
		}

		await handler.HandleAsync(context, message, cancellationToken).ConfigureAwait(false);
	}

	private Task UnknownAsync(string actionType, DataFlowChangeContext context)
	{
		_logger.LogWarning(
			"Ignoring unknown DataFlowChange action type '{ActionType}'. DataFlowExecutionId={DataFlowExecutionId} DataFlowVersion={DataFlowVersion}",
			actionType,
			context.DataFlowExecutionId,
			context.NewVersion);
		return Task.CompletedTask;
	}
}
