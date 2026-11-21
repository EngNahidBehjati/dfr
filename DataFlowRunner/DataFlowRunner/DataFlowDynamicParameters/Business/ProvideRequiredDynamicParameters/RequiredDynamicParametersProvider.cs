using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Infrastructure;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.ProvideRequiredDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;

namespace DataFlowRunner.DataFlowDynamicParameters.Business.ProvideRequiredDynamicParameters;

internal sealed class RequiredDynamicParametersProvider : IRequiredDynamicParametersProvider
{
	private readonly IDynamicParametersFetcher _dynamicParametersFetcher;
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly IDynamicParametersResponseResolver _dynamicParametersResponseResolver;
	private readonly IUpstreamSubgraphDetector _upstreamSubgraphDetector;

	public RequiredDynamicParametersProvider(
		IDynamicParametersFetcher dynamicParametersFetcher,
		IDataFlowExecutionContextProvider dataFlowExecutionContextProvider,
		IDynamicParametersResponseResolver dynamicParametersResponseResolver,
		IUpstreamSubgraphDetector upstreamSubgraphDetector)
	{
		_dynamicParametersFetcher = dynamicParametersFetcher ??
		                                      throw new ArgumentNullException(nameof(dynamicParametersFetcher));
		_dataFlowExecutionContextProvider = dataFlowExecutionContextProvider ??
		                                    throw new ArgumentNullException(nameof(dataFlowExecutionContextProvider));
		_dynamicParametersResponseResolver = dynamicParametersResponseResolver ??
		                                     throw new ArgumentNullException(nameof(dynamicParametersResponseResolver));
		_upstreamSubgraphDetector = upstreamSubgraphDetector ??
		                            throw new ArgumentNullException(nameof(upstreamSubgraphDetector));
	}

	public async Task<List<DynamicParameterDto>> ProvideDataFlowDynamicParametersAsync(Guid dataFlowExecutionId,
		long dataFlowVersion,
		CancellationToken cancellationToken)
	{
		var executionId = new DataFlowExecutionId(dataFlowExecutionId);

		DataFlowExecutionContext dataFlowExecutionContext = await _dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(executionId, cancellationToken).ConfigureAwait(false);

		ResponseDataFlowDynamicParametersDto response = await _dynamicParametersFetcher
			.FetchDataFlowDynamicParametersAsync(
				executionId,
				[],
				cancellationToken).ConfigureAwait(false);

		return _dynamicParametersResponseResolver.ResolveForFlow(executionId, dataFlowExecutionContext.Graph, response);
	}

	public async Task<List<DynamicParameterDto>> ProvideDataFlowDynamicParametersForRunningNodeAsync(
		Guid dataFlowExecutionId,
		Guid nodeId,
		long dataFlowVersion,
		CancellationToken cancellationToken)
	{
		var executionId = new DataFlowExecutionId(dataFlowExecutionId);
		var selectedNodeId = new NodeId(nodeId);

		DataFlowExecutionContext context = await _dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(executionId, cancellationToken).ConfigureAwait(false);

		List<NodeId> upstreamNodeIds = GetUpstreamNodeIds(context, executionId, selectedNodeId);

		ResponseDataFlowDynamicParametersDto response = await _dynamicParametersFetcher
			.FetchDataFlowDynamicParametersAsync(
				executionId,
				upstreamNodeIds,
				cancellationToken).ConfigureAwait(false);

		return _dynamicParametersResponseResolver.ResolveForNode(
			executionId,
			selectedNodeId,
			context.Graph,
			response);
	}

	private List<NodeId> GetUpstreamNodeIds(
		DataFlowExecutionContext dataFlowExecutionContext,
		DataFlowExecutionId executionId,
		NodeId nodeId)
	{
		IReadOnlyGraph upstreamSubgraph = _upstreamSubgraphDetector.DetectUpstreamSubgraph(
			dataFlowExecutionContext.Graph,
			executionId,
			nodeId);

		return upstreamSubgraph.GetAllNodes()
			.Select(node => node.Id)
			.ToList();
	}
}
