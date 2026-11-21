using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecution.Business;

public class GraphExecutionEligibilityCheckerTests
{
	private readonly INodeExecutionStateStore _nodeExecutionStateStore;
	private readonly INodeExecutionStateClassifier _nodeExecutionStateClassifier;
	private readonly GraphExecutionEligibilityChecker _sut;

	public GraphExecutionEligibilityCheckerTests()
	{
		_nodeExecutionStateStore = Substitute.For<INodeExecutionStateStore>();
		_nodeExecutionStateClassifier = Substitute.For<INodeExecutionStateClassifier>();
		_sut = new GraphExecutionEligibilityChecker(_nodeExecutionStateStore, _nodeExecutionStateClassifier);
	}

	[Fact]
	public void IsGraphExecutable_ShouldReturnTrue_WhenAllNodeStatesAreRunnable()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		var nodeA = new Node { Id = new NodeId(Guid.NewGuid()) };
		var nodeB = new Node { Id = new NodeId(Guid.NewGuid()) };
		var nodes = new HashSet<Node> { nodeA, nodeB };
		graph.GetAllNodes().Returns(nodes);

		_nodeExecutionStateStore.GetNodeExecutionState(executionId, nodeA.Id)
			.Returns(NodeExecutionState.Idle);
		_nodeExecutionStateStore.GetNodeExecutionState(executionId, nodeB.Id)
			.Returns(NodeExecutionState.Canceled);

		_nodeExecutionStateClassifier.IsRunnable(NodeExecutionState.Idle).Returns(true);
		_nodeExecutionStateClassifier.IsRunnable(NodeExecutionState.Canceled).Returns(true);

		// Act
		var isExecutable = _sut.IsGraphExecutable(graph, executionId);

		// Assert
		isExecutable.Should().BeTrue();
	}

	[Fact]
	public void IsGraphExecutable_ShouldReturnFalse_WhenSomeNodeStatesAreNotRunnable()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		var nodeA = new Node { Id = new NodeId(Guid.NewGuid()) };
		var nodeB = new Node { Id = new NodeId(Guid.NewGuid()) };
		var nodes = new HashSet<Node> { nodeA, nodeB };
		graph.GetAllNodes().Returns(nodes);

		_nodeExecutionStateStore.GetNodeExecutionState(executionId, nodeA.Id)
			.Returns(NodeExecutionState.Succeeded);
		_nodeExecutionStateStore.GetNodeExecutionState(executionId, nodeB.Id)
			.Returns(NodeExecutionState.Canceled);

		_nodeExecutionStateClassifier.IsRunnable(NodeExecutionState.Succeeded).Returns(false);
		_nodeExecutionStateClassifier.IsRunnable(NodeExecutionState.Canceled).Returns(true);

		// Act
		var isExecutable = _sut.IsGraphExecutable(graph, executionId);

		// Assert
		isExecutable.Should().BeFalse();
	}
}
