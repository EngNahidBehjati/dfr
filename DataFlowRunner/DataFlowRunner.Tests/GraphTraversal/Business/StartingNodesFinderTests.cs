using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Business;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.GraphTraversal.Business;

public class StartingNodesFinderTests
{
	private readonly StartingNodesFinder _sut = new();

	[Fact]
	public void GetStartingNodes_ShouldReturnNodesWithoutIncomingEdges_WhenGraphIsNotNull()
	{
		// Arrange
		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());
		var nodeC = new NodeId(Guid.NewGuid());
		var nodeD = new NodeId(Guid.NewGuid());

		var graph = Substitute.For<IReadOnlyGraph>();
		var nodes = new HashSet<Node>
		{
			CreateNode(nodeA),
			CreateNode(nodeB),
			CreateNode(nodeC),
			CreateNode(nodeD)
		};
		graph.GetAllNodes().Returns(nodes);
		var edgeByNodes = new HashSet<EdgeByNode>
		{
			new() { Id = new EdgeId(Guid.NewGuid()), FromNodeId = nodeA, ToNodeId = nodeB },
			new() { Id = new EdgeId(Guid.NewGuid()), FromNodeId = nodeB, ToNodeId = nodeC },
			new() { Id = new EdgeId(Guid.NewGuid()), FromNodeId = nodeD, ToNodeId = nodeC }
		};
		graph.GetAllEdges().Returns(edgeByNodes);

		// Act
		IReadOnlySet<NodeId> result = _sut.FindStartingNodes(graph);

		// Assert
		result.Should().BeEquivalentTo(new[] { nodeA, nodeD });
	}

	[Fact]
	public void GetStartingNodes_ShouldReturnAllNodes_WhenGraphHasNoEdges()
	{
		// Arrange
		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());

		var graph = Substitute.For<IReadOnlyGraph>();
		var nodes = new HashSet<Node> { CreateNode(nodeA), CreateNode(nodeB) };
		var edges = new HashSet<EdgeByNode>();
		graph.GetAllNodes().Returns(nodes);
		graph.GetAllEdges().Returns(edges);

		// Act
		IReadOnlySet<NodeId> result = _sut.FindStartingNodes(graph);

		// Assert
		result.Should().BeEquivalentTo(new[] { nodeA, nodeB });
	}

	private static Node CreateNode(NodeId nodeId)
	{
		var node = new Node { Id = nodeId };

		return node;
	}
}
