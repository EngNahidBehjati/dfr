using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using DataFlowRunner.DataFlowChangeEvents.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using Wolverine;

namespace DataFlowRunner.DataFlowChangeEvents.Business.Facades;

internal sealed class DataFlowChangeEventsFacade : IDataFlowChangeEventsFacade
{
	private readonly ILogger<DataFlowChangeEventsFacade> _logger;
	private readonly IActionTypeExtractor _actionTypeExtractor;
	private readonly IDataFlowChangeActionRouter _actionRouter;
	private readonly IDataFlowExecutionIdProvider _dataFlowExecutionIdProvider;
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly IDataFlowExecutionContextContainer _dataFlowExecutionContextContainer;

	public DataFlowChangeEventsFacade(
		ILogger<DataFlowChangeEventsFacade> logger,
		IActionTypeExtractor actionTypeExtractor,
		IDataFlowChangeActionRouter actionRouter,
		IDataFlowExecutionIdProvider dataFlowExecutionIdProvider,
		IDataFlowExecutionContextProvider dataFlowExecutionContextProvider,
		IDataFlowExecutionContextContainer dataFlowExecutionContextContainer)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_actionTypeExtractor = actionTypeExtractor ?? throw new ArgumentNullException(nameof(actionTypeExtractor));
		_actionRouter = actionRouter ?? throw new ArgumentNullException(nameof(actionRouter));
		_dataFlowExecutionIdProvider = dataFlowExecutionIdProvider ?? throw new ArgumentNullException(nameof(dataFlowExecutionIdProvider));
		_dataFlowExecutionContextProvider = dataFlowExecutionContextProvider ?? throw new ArgumentNullException(nameof(dataFlowExecutionContextProvider));
		_dataFlowExecutionContextContainer = dataFlowExecutionContextContainer ?? throw new ArgumentNullException(nameof(dataFlowExecutionContextContainer));
	}

	public async Task HandleAsync(ChangeEvent changeEvent, Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(changeEvent);
		ArgumentNullException.ThrowIfNull(envelope);

		if (!TryGetDataFlowExecutionId(envelope, out DataFlowExecutionId dataFlowExecutionId))
		{
			return;
		}

		if (!TryGetRunningContext(dataFlowExecutionId, out DataFlowExecutionContext? runningDataFlowExecutionContext))
		{
			return;
		}

		DataFlowChangeContext changeContext = CreateChangeContext(changeEvent, runningDataFlowExecutionContext);
		await RouteActionsAsync(changeEvent, changeContext, cancellationToken).ConfigureAwait(false);
		UpdateContextToNewVersion(changeContext);
	}

	private bool TryGetDataFlowExecutionId(Envelope envelope, out DataFlowExecutionId dataFlowExecutionId)
	{
		if (!_dataFlowExecutionIdProvider.TryGetDataFlowExecutionId(envelope, out Guid dataFlowId))
		{
			_logger.LogWarning("DataFlowChangeEvent received without a valid DataFlowExecutionId (kafka key).");
			dataFlowExecutionId = default;
			return false;
		}

		dataFlowExecutionId = new DataFlowExecutionId(dataFlowId);
		return true;
	}

	private bool TryGetRunningContext(
		DataFlowExecutionId dataFlowExecutionId,
		[NotNullWhen(true)] out DataFlowExecutionContext? runningDataFlowExecutionContext)
	{
		if (!_dataFlowExecutionContextProvider.TryGetRunningDataFlowExecutionContext(
			    dataFlowExecutionId,
			    out DataFlowExecutionContext? context))
		{
			runningDataFlowExecutionContext = null;
			return false;
		}

		runningDataFlowExecutionContext = context;
		return true;
	}

	private static DataFlowChangeContext CreateChangeContext(ChangeEvent changeEvent, DataFlowExecutionContext runningDataFlowExecutionContext)
	{
		var newVersion = new DataFlowVersion(changeEvent.DataFlowVersion);
		return new DataFlowChangeContext(
			runningDataFlowExecutionContext.DataFlowExecutionId,
			runningDataFlowExecutionContext.DataFlowVersion,
			newVersion,
			runningDataFlowExecutionContext.Graph,
			runningDataFlowExecutionContext.ProcessorInfo,
			runningDataFlowExecutionContext.ExecutionSettings);
	}

	private async Task RouteActionsAsync(
		ChangeEvent changeEvent,
		DataFlowChangeContext changeContext,
		CancellationToken cancellationToken)
	{
		foreach (JsonElement action in changeEvent.Actions)
		{
			if (!TryGetActionType(action, changeContext, out var actionType))
			{
				continue;
			}

			cancellationToken.ThrowIfCancellationRequested();
			await _actionRouter.RouteAsync(actionType, action, changeContext, cancellationToken).ConfigureAwait(false);

			UpdateContextToRunningVersion(changeContext);
		}
	}

	private bool TryGetActionType(JsonElement action, DataFlowChangeContext changeContext, out string actionType)
	{
		if (!_actionTypeExtractor.TryGetActionType(action, out var type) || string.IsNullOrWhiteSpace(type))
		{
			_logger.LogWarning(
				"Ignoring DataFlowChange action without a valid 'type' field. DataFlowExecutionId={DataFlowExecutionId} DataFlowVersion={DataFlowVersion}",
				changeContext.DataFlowExecutionId,
				changeContext.NewVersion);
			actionType = string.Empty;
			return false;
		}

		actionType = type;
		return true;
	}

	private void UpdateContextToRunningVersion(DataFlowChangeContext changeContext)
	{
		_dataFlowExecutionContextContainer.AddOrUpdateDataFlowExecutionContext(changeContext.ToRunningExecutionContext());
	}

	private void UpdateContextToNewVersion(DataFlowChangeContext changeContext)
	{
		_dataFlowExecutionContextContainer.AddOrUpdateDataFlowExecutionContext(
			changeContext.ToRunningExecutionContext() with
			{
				DataFlowVersion = changeContext.NewVersion
			});
	}
}
