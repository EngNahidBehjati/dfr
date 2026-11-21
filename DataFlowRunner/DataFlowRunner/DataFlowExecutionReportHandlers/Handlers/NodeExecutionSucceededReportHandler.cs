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

internal sealed class NodeExecutionSucceededReportHandler : INodeExecutionSucceededReportHandler
{
	private readonly INodeExecutionStateUpdater _nodeExecutionStateUpdater;
	private readonly IChildNodesExecutor _childNodesExecutor;
	private readonly ILogger<WolverineNodeExecutionReportsHandler> _logger;
	private readonly IDataFlowActionLockExecutor _actionLockExecutor;

	public NodeExecutionSucceededReportHandler(INodeExecutionStateUpdater nodeExecutionStateUpdater,
		IChildNodesExecutor childNodesExecutor,
		ILogger<WolverineNodeExecutionReportsHandler> logger,
		IDataFlowActionLockExecutor actionLockExecutor)
	{
		_nodeExecutionStateUpdater = nodeExecutionStateUpdater ?? throw new ArgumentNullException(nameof(nodeExecutionStateUpdater));
		_childNodesExecutor = childNodesExecutor ?? throw new ArgumentNullException(nameof(childNodesExecutor));
		_logger = logger  ?? throw new ArgumentNullException(nameof(logger));
		_actionLockExecutor = actionLockExecutor ?? throw new ArgumentNullException(nameof(actionLockExecutor));
	}

	public async Task HandleAsync(NodeExecutionSucceededReport report, CancellationToken cancellationToken)
	{
		var nodeId = new NodeId(report.NodeId);
		var dataFlowExecutionId = new DataFlowExecutionId(report.GraphExecutionId);

		_logger.LogInformation("Node {NodeId} succeed in GraphExecution {GraphExecutionId}",
			report.NodeId, report.GraphExecutionId);

		Func<Task> action = async () =>
		{
			_nodeExecutionStateUpdater.ChangeNodeExecutionState(dataFlowExecutionId, nodeId,
				NodeExecutionState.Succeeded);
			await _childNodesExecutor.RunReadyChildNodes(dataFlowExecutionId, nodeId, cancellationToken)
				.ConfigureAwait(false);
		};
		await _actionLockExecutor.InvokeAsync(
			dataFlowExecutionId, LockActionKeys.DataFlowExecution, action, cancellationToken).ConfigureAwait(false);
	}
}
