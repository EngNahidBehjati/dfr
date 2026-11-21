using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;
using DataFlowRunner.GraphTraversal.Business;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.GraphTraversal.Business;

public sealed class UnExecutedSubgraphDetectorTests
{
	private readonly ISubGraphCreator _subGraphCreator;
	private readonly IRunnableNodesFinder _runnableNodesFinder;
	private readonly UnExecutedSubgraphDetector _sut;

	public UnExecutedSubgraphDetectorTests()
	{
		_subGraphCreator = Substitute.For<ISubGraphCreator>();
		_runnableNodesFinder = Substitute.For<IRunnableNodesFinder>();
		_sut = new UnExecutedSubgraphDetector(_subGraphCreator, _runnableNodesFinder);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenSubGraphCreatorIsNull()
	{
		// Arrange
		ISubGraphCreator subGraphCreator = null!;

		// Act
		Action act = () => _ = new UnExecutedSubgraphDetector(subGraphCreator, _runnableNodesFinder);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenRunnableNodesFinderIsNull()
	{
		// Arrange
		IRunnableNodesFinder runnableNodesFinder = null!;

		// Act
		Action act = () => _ = new UnExecutedSubgraphDetector(_subGraphCreator, runnableNodesFinder);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void DetectUnExecutedSubgraph_ShouldReturnRunnableSubgraph_Whenever()
	{
		// Arrange
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeA = new Node { Id = new NodeId(Guid.NewGuid()) };
		var nodeB = new Node { Id = new NodeId(Guid.NewGuid()) };
		var runnableNodes = new List<Node> { nodeA, nodeB };
		_runnableNodesFinder.FindRunnableNodes(graph, executionId).Returns(runnableNodes);

		IReadOnlyGraph expectedSubgraph = Substitute.For<IReadOnlyGraph>();
		_subGraphCreator
			.CreateSubGraphFromNodes(graph,
				Arg.Is<IReadOnlyCollection<NodeId>>(ids =>
					ids.SequenceEqual(new List<NodeId>{nodeA.Id, nodeB.Id})))
			.Returns(expectedSubgraph);

		// Act
		IReadOnlyGraph result = _sut.DetectUnExecutedSubgraph(graph, executionId);

		// Assert
		result.Should().BeSameAs(expectedSubgraph);
	}
}
