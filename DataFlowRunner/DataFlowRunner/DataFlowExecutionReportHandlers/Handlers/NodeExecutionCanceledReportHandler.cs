using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;
using DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;
using DataFlowRunner.DataFlowSynchronization.Abstraction;
using DataFlowRunner.DataFlowSynchronization.Constants;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.DataFlowExecutionReportHandlers.Handlers;

internal sealed class NodeExecutionCanceledReportHandler : INodeExecutionCanceledReportHandler
{
	private readonly INodeExecutionErrorRecorder _nodeExecutionErrorRecorder;
	private readonly IPendingDownstreamNodeCanceler _pendingDownstreamNodeCanceler;
	private readonly ILogger<WolverineNodeExecutionReportsHandler> _logger;
	private readonly IDataFlowActionLockExecutor _actionLockExecutor;

	public NodeExecutionCanceledReportHandler(
		INodeExecutionErrorRecorder nodeExecutionErrorRecorder,
		IPendingDownstreamNodeCanceler pendingDownstreamNodeCanceler,
		ILogger<WolverineNodeExecutionReportsHandler> logger,
		IDataFlowActionLockExecutor actionLockExecutor)
	{
		_nodeExecutionErrorRecorder = nodeExecutionErrorRecorder ??
			throw new ArgumentNullException(nameof(nodeExecutionErrorRecorder));
		_pendingDownstreamNodeCanceler = pendingDownstreamNodeCanceler ??
			throw new ArgumentNullException(nameof(pendingDownstreamNodeCanceler));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_actionLockExecutor = actionLockExecutor ?? throw new ArgumentNullException(nameof(actionLockExecutor));
	}

	public async Task HandleAsync(NodeExecutionCanceledReport report, CancellationToken cancellationToken)
	{
		const string nodeCanceledMessage = "The plugin execution was canceled by user.";
		var nodeId = new NodeId(report.NodeId);
		var dataFlowExecutionId = new DataFlowExecutionId(report.GraphExecutionId);

		_logger.LogInformation(
			"Node {NodeId} canceled in GraphExecution {GraphExecutionId}. IsSucceeded: {IsSucceeded}. Error: {Error}",
			report.NodeId, report.GraphExecutionId, report.IsSucceeded, report.Error?.ToString());

		Func<Task> action = async () =>
		{
			_nodeExecutionErrorRecorder.RecordCancellation(
				dataFlowExecutionId, nodeId, new NodeErrorMessage { Message = nodeCanceledMessage, StackTrace = null });
			await _pendingDownstreamNodeCanceler.CancelAsync(dataFlowExecutionId, nodeId, cancellationToken)
				.ConfigureAwait(false);
		};

		await _actionLockExecutor.InvokeAsync(
			dataFlowExecutionId, LockActionKeys.DataFlowExecution, action, cancellationToken).ConfigureAwait(false);
	}
}
