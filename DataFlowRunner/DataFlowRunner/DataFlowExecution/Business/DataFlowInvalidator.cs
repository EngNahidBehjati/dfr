using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class DataFlowInvalidator : IDataFlowInvalidator
{
	private readonly IDataFlowInvalidationNodeNextState _dataFlowInvalidationNodeNextState;
	private readonly INodeExecutionRevoker _nodeExecutionRevoker;
	private readonly IDownstreamSubgraphDetector _downstreamSubgraphDetector;

	public DataFlowInvalidator(IDataFlowInvalidationNodeNextState dataFlowInvalidationNodeNextState,
		INodeExecutionRevoker nodeExecutionRevoker,
		IDownstreamSubgraphDetector downstreamSubgraphDetector)
	{
		_dataFlowInvalidationNodeNextState = dataFlowInvalidationNodeNextState ??
			throw new ArgumentNullException(nameof(dataFlowInvalidationNodeNextState));
		_nodeExecutionRevoker = nodeExecutionRevoker ??
			throw new ArgumentNullException(nameof(nodeExecutionRevoker));
		_downstreamSubgraphDetector = downstreamSubgraphDetector ??
			throw new ArgumentNullException(nameof(downstreamSubgraphDetector));
	}

	public async Task InvalidateDataFlowAsync(DataFlowExecutionContext dataFlowExecutionContext,
		CancellationToken cancellationToken)
	{
		DataFlowExecutionId dataFlowExecutionId = dataFlowExecutionContext.DataFlowExecutionId;
		var nodeIds = dataFlowExecutionContext.Graph.GetAllNodes().Select(node => node.Id).ToList();

		await _nodeExecutionRevoker
			.RevokeNodeExecution(dataFlowExecutionId, nodeIds, _dataFlowInvalidationNodeNextState, cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task InvalidateNodeAndDownstreamAsync(DataFlowExecutionContext dataFlowExecutionContext,
		NodeId sourceNodeId,
		CancellationToken cancellationToken)
	{
		DataFlowExecutionId dataFlowExecutionId = dataFlowExecutionContext.DataFlowExecutionId;

		IReadOnlySet<NodeId> downstreamNodeIds =
			_downstreamSubgraphDetector.DetectDownstreamNodes(
				dataFlowExecutionContext.Graph,
				dataFlowExecutionId,
				sourceNodeId,
				IsNotNeededToInvalidateNode);

		HashSet<NodeId> nodeIdsToRevoke = [sourceNodeId, ..downstreamNodeIds];

		await _nodeExecutionRevoker
			.RevokeNodeExecution(dataFlowExecutionId, nodeIdsToRevoke, _dataFlowInvalidationNodeNextState,
				cancellationToken)
			.ConfigureAwait(false);
	}

	private bool IsNotNeededToInvalidateNode(NodeExecutionState state)
	{
		return state is NodeExecutionState.Idle
			or NodeExecutionState.Failed
			or NodeExecutionState.Canceled
			or NodeExecutionState.Canceling;
	}
}
