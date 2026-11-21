using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class GraphExecutionSetupService : IGraphExecutionSetupService
{
	private readonly INodeConfigInitializer _nodeConfigInitializer;
	private readonly INodeExecutionStateUpdater _nodeExecutionStateUpdater;
	private readonly INodeExecutorEngineGraphInitializer _nodeExecutorEngineGraphInitializer;
	private readonly IDataFlowExecutionStatusStore _dataFlowExecutionStatusStore;

	public GraphExecutionSetupService(
		INodeConfigInitializer nodeConfigInitializer,
		INodeExecutionStateUpdater nodeExecutionStateUpdater,
		INodeExecutorEngineGraphInitializer nodeExecutorEngineGraphInitializer,
		IDataFlowExecutionStatusStore dataFlowExecutionStatusStore)
	{
		_nodeConfigInitializer = nodeConfigInitializer ?? throw new ArgumentNullException(nameof(nodeConfigInitializer));
		_nodeExecutionStateUpdater = nodeExecutionStateUpdater ?? throw new ArgumentNullException(nameof(nodeExecutionStateUpdater));
		_nodeExecutorEngineGraphInitializer = nodeExecutorEngineGraphInitializer ?? throw new ArgumentNullException(nameof(nodeExecutorEngineGraphInitializer));
		_dataFlowExecutionStatusStore = dataFlowExecutionStatusStore ?? throw new ArgumentNullException(nameof(dataFlowExecutionStatusStore));
	}

	public async Task SetupExecution(IReadOnlyGraph graph, DataFlowExecutionId dataFlowExecutionId,
		CancellationToken cancellationToken)
	{
		await _nodeExecutorEngineGraphInitializer.TryInitialize(dataFlowExecutionId, cancellationToken).ConfigureAwait(false);
		_dataFlowExecutionStatusStore.Set(dataFlowExecutionId, DataFlowExecutionStatus.DefaultStatus);
		IReadOnlyCollection<Node> allNodes = graph.GetAllNodes();
		await _nodeConfigInitializer.EnsureNodeConfigsLoadedAsync(dataFlowExecutionId, allNodes, cancellationToken).ConfigureAwait(false);
		ChangeNodeStatesToPending(dataFlowExecutionId, allNodes);
	}

	private void ChangeNodeStatesToPending(DataFlowExecutionId dataFlowExecutionId, IReadOnlyCollection<Node> nodeIds)
	{
		foreach (Node nodeId in nodeIds)
		{
			_nodeExecutionStateUpdater.ChangeNodeExecutionState(dataFlowExecutionId, nodeId.Id, NodeExecutionState.Pending);
		}
	}
}
