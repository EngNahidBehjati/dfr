using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;
using DataFlowRunner.Tests.Common;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecution.Business;

public sealed class DataFlowInvalidatorTests
{
	private readonly IDataFlowInvalidationNodeNextState _nodeNextState;
	private readonly INodeExecutionRevoker _nodeExecutionRevoker;
	private readonly IDownstreamSubgraphDetector _downstreamSubgraphDetector;
	private readonly DataFlowInvalidator _sut;

	public DataFlowInvalidatorTests()
	{
		_nodeNextState = Substitute.For<IDataFlowInvalidationNodeNextState>();
		_nodeExecutionRevoker = Substitute.For<INodeExecutionRevoker>();
		_downstreamSubgraphDetector = Substitute.For<IDownstreamSubgraphDetector>();
		_sut = new DataFlowInvalidator(_nodeNextState, _nodeExecutionRevoker, _downstreamSubgraphDetector);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenNodeNextStateIsNull()
	{
		// Arrange
		IDataFlowInvalidationNodeNextState nodeNextState = null!;

		// Act
		Action act = () => _ = new DataFlowInvalidator(nodeNextState, _nodeExecutionRevoker, _downstreamSubgraphDetector);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("dataFlowInvalidationNodeNextState");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenNodeExecutionRevokerIsNull()
	{
		// Arrange
		INodeExecutionRevoker nodeExecutionRevoker = null!;

		// Act
		Action act = () => _ = new DataFlowInvalidator(_nodeNextState, nodeExecutionRevoker, _downstreamSubgraphDetector);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("nodeExecutionRevoker");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDownstreamSubgraphDetectorIsNull()
	{
		// Arrange
		IDownstreamSubgraphDetector downstreamSubgraphDetector = null!;

		// Act
		Action act = () => _ = new DataFlowInvalidator(_nodeNextState, _nodeExecutionRevoker, downstreamSubgraphDetector);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("downstreamSubgraphDetector");
	}

	[Fact]
	public async Task InvalidateDataFlowAsync_ShouldRevokeAllGraphNodeIds_WhenInvalidatingEntireDataFlow()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var node = new Node { Id = new NodeId(Guid.NewGuid()) };
		List<Node> nodes = [node];

		IGraph graph = Substitute.For<IGraph>();
		graph.GetAllNodes().Returns(nodes);

		var context = new DataFlowExecutionContext
		{
			DataFlowExecutionId = executionId,
			Graph = graph,
			DataFlowVersion = new DataFlowVersion(12),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		// Act
		await _sut.InvalidateDataFlowAsync(context, CancellationToken.None);

		// Assert
		await _nodeExecutionRevoker.Received(1)
			.RevokeNodeExecution(
				executionId,
				Arg.Is<IReadOnlyCollection<NodeId>>(ids => ids.SequenceEqual(new[] { node.Id })),
				_nodeNextState,
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task InvalidateNodeAndDownstreamAsync_ShouldRevokeSourceAndDownstreamNodeIds_WhenDownstreamNodesExist()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var sourceNodeId = new NodeId(Guid.NewGuid());
		var downstreamNodeId1 = new NodeId(Guid.NewGuid());
		var downstreamNodeId2 = new NodeId(Guid.NewGuid());

		var sourceNode = new Node { Id = sourceNodeId };
		var downstreamNode1 = new Node { Id = downstreamNodeId1 };
		var downstreamNode2 = new Node { Id = downstreamNodeId2 };

		IGraph graph = Substitute.For<IGraph>();
		graph.GetAllNodes().Returns(new List<Node> { sourceNode, downstreamNode1, downstreamNode2 });

		var context = new DataFlowExecutionContext
		{
			DataFlowExecutionId = executionId,
			Graph = graph,
			DataFlowVersion = new DataFlowVersion(1),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		_downstreamSubgraphDetector
			.DetectDownstreamNodes(graph, executionId, sourceNodeId, Arg.Any<Func<NodeExecutionState, bool>>())
			.Returns(new HashSet<NodeId> { downstreamNodeId1, downstreamNodeId2 });

		// Act
		await _sut.InvalidateNodeAndDownstreamAsync(context, sourceNodeId, CancellationToken.None);

		// Assert
		await _nodeExecutionRevoker.Received(1)
			.RevokeNodeExecution(
				executionId,
				Arg.Is<IReadOnlyCollection<NodeId>>(nodeIds =>
					nodeIds.ToHashSet().SetEquals(new HashSet<NodeId>
						{ sourceNodeId, downstreamNodeId1, downstreamNodeId2 })),
				_nodeNextState,
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task InvalidateNodeAndDownstreamAsync_ShouldRevokeOnlySourceNode_WhenNoDownstreamNodesExist()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var sourceNodeId = new NodeId(Guid.NewGuid());
		var sourceNode = new Node { Id = sourceNodeId };

		IGraph graph = Substitute.For<IGraph>();
		graph.GetAllNodes().Returns(new List<Node> { sourceNode });

		var context = new DataFlowExecutionContext
		{
			DataFlowExecutionId = executionId,
			Graph = graph,
			DataFlowVersion = new DataFlowVersion(1),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		_downstreamSubgraphDetector
			.DetectDownstreamNodes(graph, executionId, sourceNodeId, Arg.Any<Func<NodeExecutionState, bool>>())
			.Returns(new HashSet<NodeId>());

		// Act
		await _sut.InvalidateNodeAndDownstreamAsync(context, sourceNodeId, CancellationToken.None);

		// Assert
		await _nodeExecutionRevoker.Received(1)
			.RevokeNodeExecution(
				executionId,
				Arg.Is<IReadOnlyCollection<NodeId>>(nodeIds =>
					nodeIds.Count == 1 && nodeIds.Single() == sourceNodeId),
				_nodeNextState,
				Arg.Any<CancellationToken>());
	}
}
