using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;
using DataFlowRunner.GraphTraversal.Business;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.GraphTraversal.Business;

public sealed class DownstreamSubgraphDetectorTests
{
	private readonly IChildNodesFinder _childNodesFinder;

	private readonly INodeExecutionStateProvider _nodeExecutionStateProvider;

	private readonly DownstreamSubgraphDetector _sut;

	public DownstreamSubgraphDetectorTests()
	{
		_childNodesFinder = Substitute.For<IChildNodesFinder>();
		_nodeExecutionStateProvider = Substitute.For<INodeExecutionStateProvider>();
		_sut = new DownstreamSubgraphDetector(_childNodesFinder, _nodeExecutionStateProvider);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenChildNodesFinderIsNull()
	{
		// Arrange
		IChildNodesFinder childNodesFinder = null!;

		// Act
		Action act = () => _ = new DownstreamSubgraphDetector(childNodesFinder, _nodeExecutionStateProvider);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("childNodesFinder");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenNodeExecutionStateProviderIsNull()
	{
		// Arrange
		INodeExecutionStateProvider nodeExecutionStateProvider = null!;

		// Act
		Action act = () => _ = new DownstreamSubgraphDetector(_childNodesFinder, nodeExecutionStateProvider);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("nodeExecutionStateProvider");
	}

	[Fact]
	public void DetectDownstreamNodes_ShouldReturnOnlyPendingNodes_WhenNonPendingStopConditionIsApplied()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var sourceNodeId = new NodeId(Guid.NewGuid());
		var childNodeId1 = new NodeId(Guid.NewGuid());
		var childNodeId2 = new NodeId(Guid.NewGuid());
		var childNodeId3 = new NodeId(Guid.NewGuid());

		IGraph graph = Substitute.For<IGraph>();

		var childNodes = new HashSet<NodeId> { childNodeId1, childNodeId2, childNodeId3 };
		_childNodesFinder.GetChildNodeIds(sourceNodeId, graph)
			.Returns(childNodes);

		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, childNodeId1)
			.Returns(NodeExecutionState.Pending);
		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, childNodeId2)
			.Returns(NodeExecutionState.Pending);
		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, childNodeId3)
			.Returns(NodeExecutionState.Running);

		// Act
		IReadOnlySet<NodeId> result = _sut.DetectDownstreamNodes(
			graph, executionId, sourceNodeId, state => state is not NodeExecutionState.Pending);

		// Assert
		result.Should().BeEquivalentTo([childNodeId1, childNodeId2]);
	}

	[Fact]
	public void DetectDownstreamNodes_ShouldReturnEmpty_WhenDownstreamNodesMatchStopWhenCondition()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var sourceNodeId = new NodeId(Guid.NewGuid());
		var childNodeId1 = new NodeId(Guid.NewGuid());
		var childNodeId2 = new NodeId(Guid.NewGuid());

		IGraph graph = Substitute.For<IGraph>();

		var childNodes = new HashSet<NodeId> { childNodeId1, childNodeId2 };
		_childNodesFinder.GetChildNodeIds(sourceNodeId, graph)
			.Returns(childNodes);

		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, childNodeId1)
			.Returns(NodeExecutionState.Running);
		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, childNodeId2)
			.Returns(NodeExecutionState.Succeeded);

		// Act
		IReadOnlySet<NodeId> result = _sut.DetectDownstreamNodes(
			graph, executionId, sourceNodeId, state => state is not NodeExecutionState.Pending);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void DetectDownstreamNodes_ShouldVisitSharedChildOnce_WhenMultiplePendingParentsShareChild()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var sourceNodeId = new NodeId(Guid.NewGuid());
		var childNodeId1 = new NodeId(Guid.NewGuid());
		var childNodeId2 = new NodeId(Guid.NewGuid());
		var childNodeId3 = new NodeId(Guid.NewGuid());
		var grandChildNodeId1 = new NodeId(Guid.NewGuid());
		var grandChildNodeId2 = new NodeId(Guid.NewGuid());

		// Source -> childNodeId1 (Pending) -> grandChildNodeId1 (Pending), grandChildNodeId2 (Pending)
		// Source -> childNodeId2 (Pending) -> grandChildNodeId1 (Pending)
		// Source -> childNodeId3 (Running)

		IGraph graph = Substitute.For<IGraph>();

		_childNodesFinder.GetChildNodeIds(sourceNodeId, graph)
			.Returns(new HashSet<NodeId> { childNodeId1, childNodeId2, childNodeId3 });

		_childNodesFinder.GetChildNodeIds(childNodeId1, graph).Returns(
			new HashSet<NodeId> { grandChildNodeId1, grandChildNodeId2 });
		_childNodesFinder.GetChildNodeIds(childNodeId2, graph).Returns(
			new HashSet<NodeId> { grandChildNodeId1 });
		_childNodesFinder.GetChildNodeIds(childNodeId3, graph).Returns(
			new HashSet<NodeId>());

		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, childNodeId1)
			.Returns(NodeExecutionState.Pending);
		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, childNodeId2)
			.Returns(NodeExecutionState.Pending);
		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, childNodeId3)
			.Returns(NodeExecutionState.Running);
		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, grandChildNodeId1)
			.Returns(NodeExecutionState.Pending);
		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, grandChildNodeId2)
			.Returns(NodeExecutionState.Pending);

		// Act
		IReadOnlySet<NodeId> result = _sut.DetectDownstreamNodes(
			graph, executionId, sourceNodeId, state => state is not NodeExecutionState.Pending);

		// Assert
		result.Should().BeEquivalentTo([childNodeId1, childNodeId2, grandChildNodeId1, grandChildNodeId2]);
	}

	[Fact]
	public void DetectDownstreamNodes_ShouldStopTraversal_WhenNodeMatchStopCondition()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var sourceNodeId = new NodeId(Guid.NewGuid());
		var childNodeId1 = new NodeId(Guid.NewGuid());
		var childNodeId2 = new NodeId(Guid.NewGuid());
		var childOfNode1 = new NodeId(Guid.NewGuid());
		var childOfNode2 = new NodeId(Guid.NewGuid());

		// Source -> childNodeId1 (Pending) -> childOfNode1 (Pending)
		// Source -> childNodeId2 (Running) -> childOfNode2 (Pending) - should not check childOfNode2

		IGraph graph = Substitute.For<IGraph>();

		_childNodesFinder.GetChildNodeIds(sourceNodeId, graph)
			.Returns(new HashSet<NodeId> { childNodeId1, childNodeId2 });

		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, childNodeId1)
			.Returns(NodeExecutionState.Pending);
		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, childNodeId2)
			.Returns(NodeExecutionState.Running);

		_childNodesFinder.GetChildNodeIds(childNodeId1, graph)
			.Returns(new HashSet<NodeId> { childOfNode1 });
		_childNodesFinder.GetChildNodeIds(childNodeId2, graph)
			.Returns(new HashSet<NodeId> { childOfNode2 });

		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, childOfNode1)
			.Returns(NodeExecutionState.Pending);
		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, childOfNode2)
			.Returns(NodeExecutionState.Pending);

		// Act
		IReadOnlySet<NodeId> result = _sut.DetectDownstreamNodes(
			graph, executionId, sourceNodeId, state => state is not NodeExecutionState.Pending);

		// Assert
		result.Should().BeEquivalentTo([childNodeId1, childOfNode1]);
	}

	[Fact]
	public void DetectDownstreamNodes_ShouldReturnAllReachableDownstreamNodeIds_WhenNodeHasNestedChildren()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var sourceNodeId = new NodeId(Guid.NewGuid());
		var childNodeId1 = new NodeId(Guid.NewGuid());
		var childNodeId2 = new NodeId(Guid.NewGuid());
		var grandChildNodeId = new NodeId(Guid.NewGuid());

		IGraph graph = Substitute.For<IGraph>();
		_childNodesFinder.GetChildNodeIds(sourceNodeId, graph)
			.Returns(new HashSet<NodeId> { childNodeId1, childNodeId2 });
		_childNodesFinder.GetChildNodeIds(childNodeId1, graph)
			.Returns(new HashSet<NodeId> { grandChildNodeId });
		_childNodesFinder.GetChildNodeIds(childNodeId2, graph)
			.Returns(new HashSet<NodeId>());
		_childNodesFinder.GetChildNodeIds(grandChildNodeId, graph)
			.Returns(new HashSet<NodeId>());

		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, Arg.Any<NodeId>())
			.Returns(NodeExecutionState.Running);

		// Act
		IReadOnlySet<NodeId> result = _sut.DetectDownstreamNodes(graph, executionId, sourceNodeId, _ => false);

		// Assert
		result.Should().BeEquivalentTo([childNodeId1, childNodeId2, grandChildNodeId]);
	}

	[Fact]
	public void DetectDownstreamNodes_ShouldReturnEmpty_WhenNodeHasNoChildren()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var sourceNodeId = new NodeId(Guid.NewGuid());
		IGraph graph = Substitute.For<IGraph>();
		_childNodesFinder.GetChildNodeIds(sourceNodeId, graph).Returns(new HashSet<NodeId>());

		// Act
		IReadOnlySet<NodeId> result = _sut.DetectDownstreamNodes(graph, executionId, sourceNodeId, _ => false);

		// Assert
		result.Should().BeEmpty();
	}
}
