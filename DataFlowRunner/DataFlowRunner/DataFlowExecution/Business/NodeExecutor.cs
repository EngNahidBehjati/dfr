using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.DataFlowNodeConfigs.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class NodeExecutor : INodeExecutor
{
	private readonly INodeExecutionStateUpdater _nodeExecutionStateUpdater;
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly IDataFlowNodeConfigsStore _nodeConfigsStore;
	private readonly IDataFlowExecutionUserIdProvider _dataFlowExecutionUserIdProvider;
	private readonly IDataFlowDynamicParametersStore _dynamicParametersStore;

	public NodeExecutor(INodeExecutionStateUpdater nodeExecutionStateUpdater,
		IServiceScopeFactory serviceScopeFactory,
		IDataFlowNodeConfigsStore nodeConfigsStore,
		IDataFlowExecutionUserIdProvider dataFlowExecutionUserIdProvider,
		IDataFlowDynamicParametersStore dynamicParametersStore)
	{
		_nodeExecutionStateUpdater = nodeExecutionStateUpdater ?? throw new ArgumentNullException(nameof(nodeExecutionStateUpdater));
		_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		_nodeConfigsStore = nodeConfigsStore ?? throw new ArgumentNullException(nameof(nodeConfigsStore));
		_dataFlowExecutionUserIdProvider = dataFlowExecutionUserIdProvider ?? throw new ArgumentNullException(nameof(dataFlowExecutionUserIdProvider));
		_dynamicParametersStore = dynamicParametersStore ?? throw new ArgumentNullException(nameof(dynamicParametersStore));
	}

	public async Task ExecuteNodesAsync(DataFlowExecutionId dataFlowExecutionId,
		IReadOnlySet<NodeId> nodeIds,
		CancellationToken cancellationToken)
	{
		foreach (NodeId nodeId in nodeIds)
		{
			_nodeExecutionStateUpdater.ChangeNodeExecutionState(dataFlowExecutionId, nodeId, NodeExecutionState.Running);
		}

		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IDataFlowExecutionCommandPublisher dataFlowExecutionCommandPublisher =
			scope.ServiceProvider.GetRequiredService<IDataFlowExecutionCommandPublisher>();
		UserId userId = _dataFlowExecutionUserIdProvider.GetDataFlowExecutionUserId(dataFlowExecutionId);

		foreach (NodeId nodeId in nodeIds)
		{
			var dynamicParameters =
				_dynamicParametersStore.GetDynamicParameters(dataFlowExecutionId, nodeId)
					.Select(parameter => new ExecutionDynamicParameter {Name = parameter.Name, Value = parameter.Value})
					.ToList();

			// TODO: Fill PortConnections. For now NodeExecutorEngine does not need that and get port connections from config.
			var executeNodeCommand = new ExecuteNodeCommand
			{
				GraphExecutionId = dataFlowExecutionId.Value,
				UserId = userId.Value,
				NodeId = nodeId.Value,
				Config = _nodeConfigsStore.GetNodeConfig(dataFlowExecutionId, nodeId).Content,
				PortConnections = null,
				DynamicParameters = dynamicParameters
			};

			await dataFlowExecutionCommandPublisher.PublishCommandAsync(executeNodeCommand, cancellationToken)
				.ConfigureAwait(false);
		}
	}
}
