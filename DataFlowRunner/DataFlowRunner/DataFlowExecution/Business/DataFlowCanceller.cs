using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class DataFlowCanceller : IDataFlowCanceller
{
	private readonly IDataFlowCancellationNodeNextState _dataFlowCancellationNodeNextState;
	private readonly INodeExecutionRevoker _nodeExecutionRevoker;
	private readonly IDownstreamSubgraphDetector _downstreamSubgraphDetector;

	public DataFlowCanceller(IDataFlowCancellationNodeNextState dataFlowCancellationNodeNextState,
		INodeExecutionRevoker nodeExecutionRevoker,
		IDownstreamSubgraphDetector downstreamSubgraphDetector)
	{
		_dataFlowCancellationNodeNextState = dataFlowCancellationNodeNextState ??
		                                      throw new ArgumentNullException(nameof(dataFlowCancellationNodeNextState));
		_nodeExecutionRevoker = nodeExecutionRevoker ??
		                                    throw new ArgumentNullException(nameof(nodeExecutionRevoker));
		_downstreamSubgraphDetector = downstreamSubgraphDetector ??
		                              throw new ArgumentNullException(nameof(downstreamSubgraphDetector));
	}

	public async Task CancelDataFlowAsync(DataFlowExecutionContext dataFlowExecutionContext,
		CancellationToken cancellationToken)
	{
		DataFlowExecutionId dataFlowExecutionId = dataFlowExecutionContext.DataFlowExecutionId;
		var nodeIds = dataFlowExecutionContext.Graph.GetAllNodes().Select(node => node.Id).ToList();

		await _nodeExecutionRevoker
			.RevokeNodeExecution(dataFlowExecutionId, nodeIds, _dataFlowCancellationNodeNextState, cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task CancelNodeAndDownstreamAsync(DataFlowExecutionContext dataFlowExecutionContext,
		NodeId sourceNodeId,
		CancellationToken cancellationToken)
	{
		DataFlowExecutionId dataFlowExecutionId = dataFlowExecutionContext.DataFlowExecutionId;

		IReadOnlySet<NodeId> downstreamNodeIds =
			_downstreamSubgraphDetector.DetectDownstreamNodes(
				dataFlowExecutionContext.Graph,
				dataFlowExecutionId,
				sourceNodeId,
				IsNotNeededToCancelNode);

		HashSet<NodeId> nodeIdsToRevoke = [sourceNodeId, ..downstreamNodeIds];

		await _nodeExecutionRevoker
			.RevokeNodeExecution(dataFlowExecutionId, nodeIdsToRevoke, _dataFlowCancellationNodeNextState,
				cancellationToken)
			.ConfigureAwait(false);
	}

	private static bool IsNotNeededToCancelNode(NodeExecutionState state)
	{
		return state is NodeExecutionState.Idle
			or NodeExecutionState.Failed
			or NodeExecutionState.Canceled
			or NodeExecutionState.Canceling;
	}
}
