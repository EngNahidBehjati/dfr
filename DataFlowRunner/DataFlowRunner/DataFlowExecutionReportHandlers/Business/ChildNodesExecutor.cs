using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;

namespace DataFlowRunner.DataFlowExecutionReportHandlers.Business;

internal sealed class ChildNodesExecutor : IChildNodesExecutor
{
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly IChildNodesFinder _childNodesFinder;
	private readonly INodeExecutionReadinessChecker _nodeExecutionReadinessChecker;
	private readonly INodeExecutor _nodeExecutor;

	public ChildNodesExecutor(IDataFlowExecutionContextProvider dataFlowExecutionContextProvider,
		IChildNodesFinder childNodesFinder,
		INodeExecutionReadinessChecker nodeExecutionReadinessChecker,
		INodeExecutor nodeExecutor)
	{
		_dataFlowExecutionContextProvider = dataFlowExecutionContextProvider ?? throw new ArgumentNullException(nameof(dataFlowExecutionContextProvider));
		_childNodesFinder = childNodesFinder ?? throw new ArgumentNullException(nameof(childNodesFinder));
		_nodeExecutionReadinessChecker = nodeExecutionReadinessChecker ?? throw new ArgumentNullException(nameof(nodeExecutionReadinessChecker));
		_nodeExecutor = nodeExecutor ?? throw new ArgumentNullException(nameof(nodeExecutor));
	}

	public async Task RunReadyChildNodes(DataFlowExecutionId dataFlowExecutionId,
		NodeId nodeId,
		CancellationToken cancellationToken)
	{
		DataFlowExecutionContext dataFlowExecutionContext = await _dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(dataFlowExecutionId, cancellationToken).ConfigureAwait(false);

		var readyChildNodeIds = _childNodesFinder
			.GetChildNodeIds(nodeId, dataFlowExecutionContext.Graph)
			.Where(id => _nodeExecutionReadinessChecker.IsNodeReadyToExecute(dataFlowExecutionContext, id))
			.ToHashSet();

		await _nodeExecutor
			.ExecuteNodesAsync(dataFlowExecutionId, readyChildNodeIds, cancellationToken).ConfigureAwait(false);
	}
}
