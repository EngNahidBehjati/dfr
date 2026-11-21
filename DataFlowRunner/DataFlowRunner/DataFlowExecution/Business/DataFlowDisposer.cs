using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.DataFlowSynchronization.Abstraction;
using DataFlowRunner.DataFlowSynchronization.Constants;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class DataFlowDisposer : IDataFlowDisposer
{
	private readonly INodeExecutionStateProvider _nodeExecutionStateProvider;
	private readonly INodeExecutionStateUpdater _nodeExecutionStateUpdater;
	private readonly IDataFlowActionLockExecutor _actionLockExecutor;
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly IDataFlowExecutionUserIdProvider _dataFlowExecutionUserIdProvider;
	private readonly INodeExecutorEngineGraphInitializationTracker _graphInitializationTracker;

	public DataFlowDisposer(
		INodeExecutionStateProvider nodeExecutionStateProvider,
		INodeExecutionStateUpdater nodeExecutionStateUpdater,
		IDataFlowActionLockExecutor actionLockExecutor,
		IServiceScopeFactory serviceScopeFactory,
		IDataFlowExecutionUserIdProvider dataFlowExecutionUserIdProvider,
		INodeExecutorEngineGraphInitializationTracker nodeExecutorEngineGraphInitializationTracker)
	{
		ArgumentNullException.ThrowIfNull(nodeExecutionStateProvider);
		ArgumentNullException.ThrowIfNull(nodeExecutionStateUpdater);
		ArgumentNullException.ThrowIfNull(actionLockExecutor);
		ArgumentNullException.ThrowIfNull(serviceScopeFactory);
		ArgumentNullException.ThrowIfNull(dataFlowExecutionUserIdProvider);
		ArgumentNullException.ThrowIfNull(nodeExecutorEngineGraphInitializationTracker);

		_nodeExecutionStateProvider = nodeExecutionStateProvider;
		_nodeExecutionStateUpdater = nodeExecutionStateUpdater;
		_actionLockExecutor = actionLockExecutor;
		_serviceScopeFactory = serviceScopeFactory;
		_dataFlowExecutionUserIdProvider = dataFlowExecutionUserIdProvider;
		_graphInitializationTracker = nodeExecutorEngineGraphInitializationTracker;
	}

	public Task DisposeAsync(DataFlowExecutionId dataFlowExecutionId, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return DisposeUnderExecutionLockAsync(dataFlowExecutionId, cancellationToken);
	}

	private Task DisposeUnderExecutionLockAsync(
		DataFlowExecutionId dataFlowExecutionId,
		CancellationToken cancellationToken)
	{
		return _actionLockExecutor.InvokeAsync(
			dataFlowExecutionId,
			LockActionKeys.DataFlowExecution,
			() => PublishDisposeAndUpdateStatesAsync(dataFlowExecutionId, cancellationToken),
			cancellationToken);
	}

	private async Task PublishDisposeAndUpdateStatesAsync(
		DataFlowExecutionId dataFlowExecutionId,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		await PublishDisposeCommandAsync(dataFlowExecutionId, cancellationToken).ConfigureAwait(false);
		UpdateNodeStates(dataFlowExecutionId);
		_graphInitializationTracker.UntrackDataFlowInitialization(dataFlowExecutionId);
	}

	private void UpdateNodeStates(DataFlowExecutionId dataFlowExecutionId)
	{
		IReadOnlyDictionary<NodeId, NodeExecutionState> nodeStates =
			_nodeExecutionStateProvider.GetAllNodeExecutionStates(dataFlowExecutionId);

		foreach ((NodeId nodeId, NodeExecutionState currentState) in nodeStates)
		{
			UpdateNodeState(dataFlowExecutionId, nodeId, currentState);
		}
	}

	private void UpdateNodeState(
		DataFlowExecutionId dataFlowExecutionId,
		NodeId nodeId,
		NodeExecutionState currentState)
	{
		if (!TryGetNextState(currentState, out NodeExecutionState nextState))
		{
			return;
		}

		_nodeExecutionStateUpdater.ChangeNodeExecutionState(dataFlowExecutionId, nodeId, nextState);
	}

	private static bool TryGetNextState(NodeExecutionState currentState, out NodeExecutionState nextState)
	{
		switch (currentState)
		{
			case NodeExecutionState.Pending:
			case NodeExecutionState.Succeeded:
				nextState = NodeExecutionState.Idle;
				return true;
			case NodeExecutionState.Running:
			case NodeExecutionState.Canceling:
				nextState = NodeExecutionState.Canceled;
				return true;
			default:
				nextState = default;
				return false;
		}
	}

	private async Task PublishDisposeCommandAsync(
		DataFlowExecutionId dataFlowExecutionId,
		CancellationToken cancellationToken)
	{
		UserId userId = _dataFlowExecutionUserIdProvider.GetDataFlowExecutionUserId(dataFlowExecutionId);
		var disposeCommand = new DisposeDataFlowExecutionCommand
		{
			GraphExecutionId = dataFlowExecutionId.Value,
			UserId = userId.Value
		};

		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IDataFlowExecutionCommandPublisher commandPublisher =
			scope.ServiceProvider.GetRequiredService<IDataFlowExecutionCommandPublisher>();
		await commandPublisher.PublishCommandAsync(disposeCommand, cancellationToken).ConfigureAwait(false);
	}
}
