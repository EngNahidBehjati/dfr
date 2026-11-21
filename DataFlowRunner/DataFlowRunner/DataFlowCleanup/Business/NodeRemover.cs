using DataFlowRunner.DataFlowCleanup.Abstraction;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;

namespace DataFlowRunner.DataFlowCleanup.Business;

internal sealed class NodeRemover : INodeRemover
{
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly INodeExecutionStateStore _nodeExecutionStateStore;

	public NodeRemover(IDataFlowExecutionContextProvider dataFlowExecutionContextProvider, INodeExecutionStateStore nodeExecutionStateStore)
	{
		_dataFlowExecutionContextProvider = dataFlowExecutionContextProvider;
		_nodeExecutionStateStore = nodeExecutionStateStore;
	}

	public async Task RemoveNodesAsync(DataFlowExecutionId dataFlowExecutionId, List<NodeId> nodeIds, CancellationToken cancellationToken)
	{
		DataFlowExecutionContext executionContext = await _dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(dataFlowExecutionId, cancellationToken)
			.ConfigureAwait(false);

		executionContext.Graph.RemoveNodes(nodeIds);

		foreach (NodeId nodeId in nodeIds)
		{
			_nodeExecutionStateStore.RemoveNodeExecutionState(dataFlowExecutionId, nodeId);
		}
	}
}
