using DataFlowRunner.DataFlowDynamicParameters.Abstractions.RunDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowSynchronization.Abstraction;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;
using DataFlowRunner.Tests.Common;
using DataFlowRunner.Tests.Common.FakeClasses;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecution.Business;

public sealed class DataFlowExecutorTests
{
	private readonly IUnExecutedSubgraphDetector _unExecutedSubgraphDetector;
	private readonly IUpstreamSubgraphDetector _upstreamSubgraphDetector;
	private readonly IGraphExecutor _graphExecutor;
	private readonly IDynamicParametersStoreUpdater _dynamicParametersStoreUpdater;
	private readonly IDynamicParameterMismatchNodeInvalidator _dynamicParameterMismatchNodeInvalidator;
	private readonly IRunDynamicParametersByNodeResolver _runDynamicParametersByNodeResolver;
	private readonly DataFlowExecutor _sut;


	public DataFlowExecutorTests()
	{
		_unExecutedSubgraphDetector = Substitute.For<IUnExecutedSubgraphDetector>();
		_upstreamSubgraphDetector = Substitute.For<IUpstreamSubgraphDetector>();
		_graphExecutor = Substitute.For<IGraphExecutor>();
		_dynamicParametersStoreUpdater = Substitute.For<IDynamicParametersStoreUpdater>();
		_runDynamicParametersByNodeResolver = Substitute.For<IRunDynamicParametersByNodeResolver>();
		_dynamicParameterMismatchNodeInvalidator = Substitute.For<IDynamicParameterMismatchNodeInvalidator>();
		IDataFlowActionLockExecutor dataFlowActionLockExecutor = new FakeActionLockExecutor();

		_sut = new DataFlowExecutor(
			_unExecutedSubgraphDetector,
			_upstreamSubgraphDetector,
			_graphExecutor,
			dataFlowActionLockExecutor,
			_dynamicParameterMismatchNodeInvalidator,
			_dynamicParametersStoreUpdater,
			_runDynamicParametersByNodeResolver);
	}

	[Fact]
	public async Task RunDataFlowAsync_ShouldPrepareParametersUpdateStoreAndRunUnExecutedSubgraph_WhenCalled()
	{
		// Arrange
		IGraph graph = Substitute.For<IGraph>();
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var dataFlowVersion = new DataFlowVersion(1);
		var executionContext = new DataFlowExecutionContext
		{
			Graph = graph,
			DataFlowExecutionId = executionId,
			DataFlowVersion = dataFlowVersion,
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
		var dynamicParameters = new List<DynamicParameter>
		{
			new() { Name = "age", Value = 10 }
		};
		IReadOnlyGraph subgraph = Substitute.For<IReadOnlyGraph>();
		_unExecutedSubgraphDetector.DetectUnExecutedSubgraph(graph, executionId).Returns(subgraph);
		var nodes = new List<Node> { new() };
		graph.GetAllNodes().Returns(nodes);
		var parametersByNode = new Dictionary<NodeId, List<DynamicParameter>>
		{
			[nodes[0].Id] = dynamicParameters
		};
		_runDynamicParametersByNodeResolver
			.ResolveProvidedParametersByNodeAsync(executionId, nodes, dynamicParameters, CancellationToken.None)
			.Returns(parametersByNode);

		// Act
		await _sut.RunDataFlowAsync(executionContext, dynamicParameters, CancellationToken.None);

		// Assert
		Received.InOrder(() =>
		{
			_dynamicParameterMismatchNodeInvalidator.InvalidateNodesOnParameterMismatchAsync(
				executionContext, graph, dynamicParameters, CancellationToken.None);
			_runDynamicParametersByNodeResolver.ResolveProvidedParametersByNodeAsync(
				executionId, nodes, dynamicParameters, CancellationToken.None);
			_dynamicParametersStoreUpdater.UpdateDataFlowDynamicParameters(
				executionId, parametersByNode);
			_graphExecutor.RunGraphAsync(subgraph, executionId, CancellationToken.None);
		});
	}

	[Fact]
	public async Task RunNodeAsync_ShouldPrepareParametersUpdateStoreAndRunUnExecutedSubgraph_WhenCalled()
	{
		// Arrange
		IGraph graph = Substitute.For<IGraph>();
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var dataFlowVersion = new DataFlowVersion(1);
		var executionContext = new DataFlowExecutionContext
		{
			Graph = graph,
			DataFlowExecutionId = executionId,
			DataFlowVersion = dataFlowVersion,
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
		var dynamicParameters = new List<DynamicParameter>
		{
			new() { Name = "age", Value = 10 }
		};

		var nodeId = new NodeId(Guid.NewGuid());
		IReadOnlyGraph upstreamSubgraph = Substitute.For<IReadOnlyGraph>();
		IReadOnlyGraph unexecutedSubgraph = Substitute.For<IReadOnlyGraph>();

		var nodes = new List<Node> { new() };
		upstreamSubgraph.GetAllNodes().Returns(nodes);
		_upstreamSubgraphDetector.DetectUpstreamSubgraph(graph, executionId, nodeId)
			.Returns(upstreamSubgraph);
		_unExecutedSubgraphDetector.DetectUnExecutedSubgraph(upstreamSubgraph, executionId).Returns(unexecutedSubgraph);
		var parametersByNode = new Dictionary<NodeId, List<DynamicParameter>>
		{
			[nodes[0].Id] = dynamicParameters
		};
		_runDynamicParametersByNodeResolver
			.ResolveProvidedParametersByNodeAsync(executionId, nodes, dynamicParameters, CancellationToken.None)
			.Returns(parametersByNode);

		// Act
		await _sut.RunNodeAsync(executionContext, nodeId, dynamicParameters, CancellationToken.None);

		// Assert
		Received.InOrder(() =>
		{
			_dynamicParameterMismatchNodeInvalidator.InvalidateNodesOnParameterMismatchAsync(
				executionContext, upstreamSubgraph, dynamicParameters, CancellationToken.None);
			_runDynamicParametersByNodeResolver.ResolveProvidedParametersByNodeAsync(
				executionId, nodes, dynamicParameters, CancellationToken.None);
			_dynamicParametersStoreUpdater.UpdateDataFlowDynamicParameters(
				executionId, parametersByNode);
			_graphExecutor.RunGraphAsync(unexecutedSubgraph, executionId, CancellationToken.None);
		});
	}
}
