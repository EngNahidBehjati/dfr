using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.DataFlowExecutionReportHandlers.Business;

internal sealed class PendingDownstreamNodeCanceler : IPendingDownstreamNodeCanceler
{
	private readonly INodeExecutionErrorRecorder _nodeExecutionErrorRecorder;
	private readonly IDownstreamSubgraphDetector _downstreamSubgraphDetector;
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;

	public PendingDownstreamNodeCanceler(
		INodeExecutionErrorRecorder nodeExecutionErrorRecorder,
		IDownstreamSubgraphDetector downstreamSubgraphDetector,
		IDataFlowExecutionContextProvider dataFlowExecutionContextProvider)
	{
		_nodeExecutionErrorRecorder = nodeExecutionErrorRecorder ??
			throw new ArgumentNullException(nameof(nodeExecutionErrorRecorder));
		_downstreamSubgraphDetector = downstreamSubgraphDetector ??
			throw new ArgumentNullException(nameof(downstreamSubgraphDetector));
		_dataFlowExecutionContextProvider = dataFlowExecutionContextProvider ??
			throw new ArgumentNullException(nameof(dataFlowExecutionContextProvider));
	}

	public async Task CancelAsync(DataFlowExecutionId executionId, NodeId sourceNodeId,
		CancellationToken cancellationToken)
	{
		const string message = "The plugin execution was canceled by the previous plugin.";
		NodeErrorMessage errorMessage = new() { Message = message, StackTrace = null };

		DataFlowExecutionContext context = await _dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(executionId, cancellationToken).ConfigureAwait(false);

		IReadOnlySet<NodeId> pendingDownstreamNodes = _downstreamSubgraphDetector.DetectDownstreamNodes(
			context.Graph,
			executionId,
			sourceNodeId,
			state => state is not NodeExecutionState.Pending);

		foreach (NodeId pendingNode in pendingDownstreamNodes)
		{
			_nodeExecutionErrorRecorder.RecordCancellation(executionId, pendingNode, errorMessage);
		}
	}
}
