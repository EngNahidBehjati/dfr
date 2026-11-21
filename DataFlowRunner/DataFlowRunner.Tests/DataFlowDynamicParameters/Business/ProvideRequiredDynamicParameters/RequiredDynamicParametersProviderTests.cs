using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Infrastructure;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.ProvideRequiredDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Business.ProvideRequiredDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;
using DataFlowRunner.Tests.Common;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowDynamicParameters.Business.ProvideRequiredDynamicParameters;

public sealed class RequiredDynamicParametersProviderTests
{
	private readonly IDynamicParametersFetcher _dynamicParametersFetcher;
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly IDynamicParametersResponseResolver _dynamicParametersResponseResolver;
	private readonly IUpstreamSubgraphDetector _upstreamSubgraphDetector;
	private readonly RequiredDynamicParametersProvider _sut;

	public RequiredDynamicParametersProviderTests()
	{
		_dynamicParametersFetcher = Substitute.For<IDynamicParametersFetcher>();
		_dataFlowExecutionContextProvider = Substitute.For<IDataFlowExecutionContextProvider>();
		_dynamicParametersResponseResolver = Substitute.For<IDynamicParametersResponseResolver>();
		_upstreamSubgraphDetector = Substitute.For<IUpstreamSubgraphDetector>();
		_sut = new RequiredDynamicParametersProvider(
			_dynamicParametersFetcher,
			_dataFlowExecutionContextProvider,
			_dynamicParametersResponseResolver,
			_upstreamSubgraphDetector);
	}

	[Fact]
	public async Task ProvideDataFlowDynamicParametersAsync_ShouldFetchWithEmptyNodeListAndDelegateToResolveForFlow_WhenRunningExecutionContextIsAvailable()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		var executionId = new DataFlowExecutionId(dataFlowExecutionId);
		CancellationToken cancellationToken = CancellationToken.None;
		IGraph graph = Substitute.For<IGraph>();
		var context = new DataFlowExecutionContext
		{
			Graph = graph,
			DataFlowExecutionId = executionId,
			DataFlowVersion = new DataFlowVersion(1),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
		_dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(executionId, cancellationToken)
			.Returns(context);

		var response = new ResponseDataFlowDynamicParametersDto([]);
		var expected = new List<DynamicParameterDto>();

		_dynamicParametersFetcher
			.FetchDataFlowDynamicParametersAsync(executionId,
				Arg.Is<List<NodeId>>(x => x.Count == 0),
				cancellationToken)
			.Returns(response);
		_dynamicParametersResponseResolver
			.ResolveForFlow(executionId, graph, response)
			.Returns(expected);

		// Act
		List<DynamicParameterDto> actual =
			await _sut.ProvideDataFlowDynamicParametersAsync(dataFlowExecutionId, dataFlowVersion, cancellationToken);

		// Assert
		actual.Should().BeSameAs(expected);
	}

	[Fact]
	public async Task ProvideDataFlowDynamicParametersForRunningNodeAsync_ShouldPassUpstreamIdsToFetcherAndDelegateToResolveForNode_WhenUpstreamSubgraphIsDetected()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var upstreamNodeId = Guid.NewGuid();
		const long dataFlowVersion = 2;
		var executionId = new DataFlowExecutionId(dataFlowExecutionId);
		var selectedNodeId = new NodeId(nodeId);
		CancellationToken cancellationToken = CancellationToken.None;

		IGraph graph = Substitute.For<IGraph>();
		IReadOnlyGraph upstreamSubgraph = Substitute.For<IReadOnlyGraph>();
		upstreamSubgraph.GetAllNodes().Returns(
		[
			new Node { Id = selectedNodeId },
			new Node { Id = new NodeId(upstreamNodeId) }
		]);
		var context = new DataFlowExecutionContext
		{
			Graph = graph,
			DataFlowExecutionId = executionId,
			DataFlowVersion = new DataFlowVersion(dataFlowVersion),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		_dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(executionId, cancellationToken)
			.Returns(context);
		_upstreamSubgraphDetector
			.DetectUpstreamSubgraph(graph, executionId, selectedNodeId)
			.Returns(upstreamSubgraph);

		var upstreamNodeIds = new List<NodeId> { selectedNodeId, new NodeId(upstreamNodeId) };
		var response = new ResponseDataFlowDynamicParametersDto([]);
		var expected = new List<DynamicParameterDto>();

		_dynamicParametersFetcher
			.FetchDataFlowDynamicParametersAsync(executionId,
				Arg.Is<List<NodeId>>(x => x.SequenceEqual(upstreamNodeIds)),
				cancellationToken)
			.Returns(response);
		_dynamicParametersResponseResolver
			.ResolveForNode(executionId, selectedNodeId, graph, response)
			.Returns(expected);

		// Act
		List<DynamicParameterDto> actual = await _sut.ProvideDataFlowDynamicParametersForRunningNodeAsync(
			dataFlowExecutionId, nodeId, dataFlowVersion, cancellationToken);

		// Assert
		actual.Should().BeSameAs(expected);
	}
}
