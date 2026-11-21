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

internal sealed class NodeExecutionFailedReportHandler : INodeExecutionFailedReportHandler
{
	private readonly INodeExecutionErrorRecorder _nodeExecutionErrorRecorder;
	private readonly IPendingDownstreamNodeCanceler _pendingDownstreamNodeCanceler;
	private readonly ILogger<WolverineNodeExecutionReportsHandler> _logger;
	private readonly IDataFlowActionLockExecutor _actionLockExecutor;

	public NodeExecutionFailedReportHandler(
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

	public async Task HandleAsync(NodeExecutionFailedReport report, CancellationToken cancellationToken)
	{
		if (report.Error is null)
		{
			throw new InvalidOperationException(
				$"Received {nameof(NodeExecutionFailedReport)} without an error for node {report.NodeId}.");
		}

		var dataFlowExecutionId = new DataFlowExecutionId(report.GraphExecutionId);
		var nodeId = new NodeId(report.NodeId);
		NodeErrorMessage nodeErrorMessage = new() { Message = report.Error.ErrorMessage, StackTrace = report.Error.StackTrace };

		_logger.LogInformation("Node {NodeId} failed in GraphExecution {GraphExecutionId}. Error: {Error}",
			report.NodeId, report.GraphExecutionId, report.Error);

		Func<Task> action = async () =>
		{
			_nodeExecutionErrorRecorder.RecordFailure(dataFlowExecutionId, nodeId, nodeErrorMessage);
			await _pendingDownstreamNodeCanceler.CancelAsync(dataFlowExecutionId, nodeId, cancellationToken)
				.ConfigureAwait(false);
		};

		await _actionLockExecutor.InvokeAsync(
			dataFlowExecutionId, LockActionKeys.DataFlowExecution, action, cancellationToken).ConfigureAwait(false);
	}
}
