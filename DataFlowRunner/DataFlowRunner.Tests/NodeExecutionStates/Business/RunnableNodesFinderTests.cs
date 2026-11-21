using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Business;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.NodeExecutionStates.Business;

public sealed class RunnableNodesFinderTests
{
	private readonly INodeExecutionStateProvider _nodeExecutionStateProvider;
	private readonly INodeExecutionStateClassifier _nodeExecutionStateClassifier;
	private readonly RunnableNodesFinder _sut;

	public RunnableNodesFinderTests()
	{
		_nodeExecutionStateProvider = Substitute.For<INodeExecutionStateProvider>();
		_nodeExecutionStateClassifier = Substitute.For<INodeExecutionStateClassifier>();
		_sut = new RunnableNodesFinder(_nodeExecutionStateProvider, _nodeExecutionStateClassifier);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenNodeExecutionStateProviderIsNull()
	{
		// Arrange
		INodeExecutionStateProvider provider = null!;

		// Act
		Action act = () => _ = new RunnableNodesFinder(provider, _nodeExecutionStateClassifier);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenNodeExecutionStateClassifierIsNull()
	{
		// Arrange
		INodeExecutionStateClassifier classifier = null!;

		// Act
		Action act = () => _ = new RunnableNodesFinder(_nodeExecutionStateProvider, classifier);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void FindRunnableNodes_ShouldReturnRunnableNodes_WhenThereAreSomeRunnableNodes()
	{
		// Arrange
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeA = new Node { Id = new NodeId(Guid.NewGuid()) };
		var nodeB = new Node { Id = new NodeId(Guid.NewGuid()) };
		var nodeC = new Node { Id = new NodeId(Guid.NewGuid()) };
		graph.GetAllNodes().Returns(new List<Node> { nodeA, nodeB, nodeC });

		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, nodeA.Id)
			.Returns(NodeExecutionState.Canceled);
		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, nodeB.Id)
			.Returns(NodeExecutionState.Idle);
		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, nodeC.Id)
			.Returns(NodeExecutionState.Pending);

		_nodeExecutionStateClassifier.IsRunnable(NodeExecutionState.Canceled).Returns(true);
		_nodeExecutionStateClassifier.IsRunnable(NodeExecutionState.Idle).Returns(true);
		_nodeExecutionStateClassifier.IsRunnable(NodeExecutionState.Pending).Returns(false);

		// Act
		List<Node> result = _sut.FindRunnableNodes(graph, executionId);

		// Assert
		result.Should().BeEquivalentTo([nodeA, nodeB]);
	}

	[Fact]
	public void FindRunnableNodes_ShouldReturnEmptyList_WhenNoNodesAreRunnable()
	{
		// Arrange
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		var executionId = new DataFlowExecutionId(Guid.NewGuid());

		var nodeA = new Node { Id = new NodeId(Guid.NewGuid()) };
		graph.GetAllNodes().Returns(new List<Node> { nodeA });

		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, nodeA.Id)
			.Returns(NodeExecutionState.Idle);
		_nodeExecutionStateClassifier.IsRunnable(NodeExecutionState.Idle)
			.Returns(false);

		// Act
		List<Node> result = _sut.FindRunnableNodes(graph, executionId);

		// Assert
		result.Should().BeEmpty();
	}
}
