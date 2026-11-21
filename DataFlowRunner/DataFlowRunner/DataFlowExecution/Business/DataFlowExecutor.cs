using DataFlowRunner.DataFlowDynamicParameters.Abstractions.RunDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowSynchronization.Abstraction;
using DataFlowRunner.DataFlowSynchronization.Constants;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class DataFlowExecutor : IDataFlowExecutor
{
	private readonly IUnExecutedSubgraphDetector _unExecutedSubgraphDetector;
	private readonly IUpstreamSubgraphDetector _upstreamSubgraphDetector;
	private readonly IGraphExecutor _graphExecutor;
	private readonly IDataFlowActionLockExecutor _actionLockExecutor;
	private readonly IDynamicParametersStoreUpdater _dynamicParametersStoreUpdater;
	private readonly IDynamicParameterMismatchNodeInvalidator _dynamicParameterMismatchNodeInvalidator;
	private readonly IRunDynamicParametersByNodeResolver _runDynamicParametersByNodeResolver;

	public DataFlowExecutor(IUnExecutedSubgraphDetector unExecutedSubgraphDetector,
		IUpstreamSubgraphDetector upstreamSubgraphDetector,
		IGraphExecutor graphExecutor,
		IDataFlowActionLockExecutor actionLockExecutor,
		IDynamicParameterMismatchNodeInvalidator dynamicParameterMismatchNodeInvalidator,
		IDynamicParametersStoreUpdater dynamicParametersStoreUpdater,
		IRunDynamicParametersByNodeResolver runDynamicParametersByNodeResolver)
	{
		_unExecutedSubgraphDetector = unExecutedSubgraphDetector ?? throw new ArgumentNullException(nameof(unExecutedSubgraphDetector));
		_upstreamSubgraphDetector = upstreamSubgraphDetector ?? throw new ArgumentNullException(nameof(upstreamSubgraphDetector));
		_graphExecutor = graphExecutor ?? throw new ArgumentNullException(nameof(graphExecutor));
		_actionLockExecutor = actionLockExecutor ?? throw new ArgumentNullException(nameof(actionLockExecutor));
		_dynamicParameterMismatchNodeInvalidator = dynamicParameterMismatchNodeInvalidator ?? throw new ArgumentNullException(nameof(dynamicParameterMismatchNodeInvalidator));
		_dynamicParametersStoreUpdater = dynamicParametersStoreUpdater ?? throw new ArgumentNullException(nameof(dynamicParametersStoreUpdater));
		_runDynamicParametersByNodeResolver = runDynamicParametersByNodeResolver ?? throw new ArgumentNullException(nameof(runDynamicParametersByNodeResolver));
	}

	public async Task RunDataFlowAsync(DataFlowExecutionContext dataFlowExecutionContext,
		List<DynamicParameter> dynamicParameters,
		CancellationToken cancellationToken)
	{
		DataFlowExecutionId dataFlowExecutionId = dataFlowExecutionContext.DataFlowExecutionId;
		IGraph graph = dataFlowExecutionContext.Graph;

		await _dynamicParameterMismatchNodeInvalidator
			.InvalidateNodesOnParameterMismatchAsync(dataFlowExecutionContext, graph, dynamicParameters, cancellationToken)
			.ConfigureAwait(false);

		await ValidateAndUpdateParameters(dataFlowExecutionId, graph, dynamicParameters, cancellationToken)
			.ConfigureAwait(false);

		Func<Task> action = async () =>
		{
			IReadOnlyGraph subgraph = _unExecutedSubgraphDetector.DetectUnExecutedSubgraph(
				graph, dataFlowExecutionId);
			await _graphExecutor.RunGraphAsync(subgraph, dataFlowExecutionId, cancellationToken).ConfigureAwait(false);
		};

		await _actionLockExecutor.InvokeAsync(
			dataFlowExecutionId, LockActionKeys.DataFlowExecution, action, cancellationToken).ConfigureAwait(false);
	}

	public async Task RunNodeAsync(DataFlowExecutionContext dataFlowExecutionContext, NodeId nodeId,
		List<DynamicParameter> dynamicParameters, CancellationToken cancellationToken)
	{
		DataFlowExecutionId dataFlowExecutionId = dataFlowExecutionContext.DataFlowExecutionId;
		IReadOnlyGraph upstreamSubgraph = _upstreamSubgraphDetector.DetectUpstreamSubgraph(
			dataFlowExecutionContext.Graph, dataFlowExecutionId, nodeId);

		await _dynamicParameterMismatchNodeInvalidator
			.InvalidateNodesOnParameterMismatchAsync(dataFlowExecutionContext, upstreamSubgraph, dynamicParameters, cancellationToken)
			.ConfigureAwait(false);

		await ValidateAndUpdateParameters(dataFlowExecutionId, upstreamSubgraph, dynamicParameters, cancellationToken)
			.ConfigureAwait(false);

		Func<Task> action = async () =>
		{
			IReadOnlyGraph unExecutedSubgraph = _unExecutedSubgraphDetector
				.DetectUnExecutedSubgraph(upstreamSubgraph, dataFlowExecutionId);
			await _graphExecutor.RunGraphAsync(unExecutedSubgraph, dataFlowExecutionId, cancellationToken)
				.ConfigureAwait(false);
		};

		await _actionLockExecutor.InvokeAsync(
			dataFlowExecutionId, LockActionKeys.DataFlowExecution, action, cancellationToken).ConfigureAwait(false);
	}

	private async Task ValidateAndUpdateParameters(DataFlowExecutionId executionId,
		IReadOnlyGraph graph, List<DynamicParameter> parameters, CancellationToken cancellationToken)
	{
		IReadOnlyCollection<Node> nodes = graph.GetAllNodes();

		IReadOnlyDictionary<NodeId, List<DynamicParameter>> parametersByNode =
			await _runDynamicParametersByNodeResolver
				.ResolveProvidedParametersByNodeAsync(executionId, nodes, parameters, cancellationToken)
				.ConfigureAwait(false);

		_dynamicParametersStoreUpdater
			.UpdateDataFlowDynamicParameters(executionId, parametersByNode);
	}
}
