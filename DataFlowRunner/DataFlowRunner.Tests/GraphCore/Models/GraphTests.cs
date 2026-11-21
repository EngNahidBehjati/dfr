using DataFlowRunner.Common.Exceptions.NotFound;
using DataFlowRunner.GraphCore.Exceptions.Conflict;
using DataFlowRunner.GraphCore.Models;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using FluentAssertions;

namespace DataFlowRunner.Tests.GraphCore.Models;

public class GraphTests
{
	[Fact]
	public void AddNodes_ShouldStoreNodes_WhenNodesDoseNotExistsBefore()
	{
		// Arrange
		var nodeId1 = new NodeId(Guid.NewGuid());
		var nodeId2 = new NodeId(Guid.NewGuid());
		Graph graph = CreateGraph();

		// Act
		graph.AddNodes(new HashSet<NodeId> { nodeId1, nodeId2 });

		// Assert
		IReadOnlyCollection<Node> nodes = graph.GetAllNodes();
		nodes.Should().HaveCount(2);
		nodes.Select(n => n.Id).Should().BeEquivalentTo([nodeId1, nodeId2]);
	}

	[Fact]
	public void AddNodes_ShouldThrowSameNodeAlreadyExistsException_WhenDuplicateNodeAdded()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());
		Graph graph = CreateGraph();

		// Act
		graph.AddNodes(new HashSet<NodeId> { nodeId });
		Action act = () => graph.AddNodes(new HashSet<NodeId> { nodeId });

		// Assert
		act.Should().Throw<DuplicateNodeException>();
	}

	[Fact]
	public void RemoveNodes_ShouldDeleteExistingNodes_WhenNodeExists()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());
		Graph graph = CreateGraph();
		graph.AddNodes(new HashSet<NodeId> { nodeId });

		// Act
		graph.RemoveNodes(new HashSet<NodeId> { nodeId });

		// Assert
		graph.GetAllNodes().Should().BeEmpty();
	}

	[Fact]
	public void NodeExists_ShouldReturnTrue_WhenNodeIsPresent()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());
		Graph graph = CreateGraph();
		graph.AddNodes(new HashSet<NodeId> { nodeId });

		// Act
		var exists = graph.NodeExists(nodeId);

		// Assert
		exists.Should().BeTrue();
	}

	[Fact]
	public void NodeExists_ShouldReturnFalse_WhenNodeIsMissing()
	{
		// Arrange
		Graph graph = CreateGraph();
		var nodeId = new NodeId(Guid.NewGuid());

		// Act
		var exists = graph.NodeExists(nodeId);

		// Assert
		exists.Should().BeFalse();
	}

	[Fact]
	public void AddEdges_ShouldStoreEdges_WhenEdgesDoseNotExistsBefore()
	{
		// Arrange
		Graph graph = CreateGraph();
		var edgeId = new EdgeId(Guid.NewGuid());
		var fromNodeId = new NodeId(Guid.NewGuid());
		var toNodeId = new NodeId(Guid.NewGuid());

		// Act
		graph.AddEdges(new HashSet<EdgeByNode>
		{
			new EdgeByNode
			{
				Id = edgeId,
				FromNodeId = fromNodeId,
				ToNodeId = toNodeId
			}
		});

		// Assert
		IReadOnlyCollection<EdgeByNode> edges = graph.GetAllEdges();
		edges.Should().HaveCount(1);
		EdgeByNode edgeByNode = edges.Single();
		edgeByNode.Id.Should().Be(edgeId);
		edgeByNode.FromNodeId.Should().Be(fromNodeId);
		edgeByNode.ToNodeId.Should().Be(toNodeId);
	}

	[Fact]
	public void AddEdges_ShouldThrowDuplicateEdgeException_WhenDuplicateEdgeAdded()
	{
		// Arrange
		Graph graph = CreateGraph();
		var edgeId = new EdgeId(Guid.NewGuid());
		var fromNodeId = new NodeId(Guid.NewGuid());
		var toNodeId = new NodeId(Guid.NewGuid());
		var edgeByNode = new EdgeByNode { Id = edgeId, FromNodeId = fromNodeId, ToNodeId = toNodeId };

		// Act
		graph.AddEdges(new HashSet<EdgeByNode> { edgeByNode });
		Action act = () => graph.AddEdges(new HashSet<EdgeByNode> { edgeByNode });

		// Assert
		act.Should().Throw<DuplicateEdgeException>();
	}

	[Fact]
	public void RemoveEdges_ShouldDeleteEdges_WhenEdgesExists()
	{
		// Arrange
		Graph graph = CreateGraph();
		var edgeId = new EdgeId(Guid.NewGuid());
		graph.AddEdges(new HashSet<EdgeByNode>
		{
			new EdgeByNode
			{
				Id = edgeId,
				FromNodeId = new NodeId(Guid.NewGuid()),
				ToNodeId = new NodeId(Guid.NewGuid())
			}
		});

		// Act
		graph.RemoveEdges(new HashSet<EdgeId> { edgeId });

		// Assert
		graph.GetAllEdges().Should().BeEmpty();
	}

	[Fact]
	public void GetEdges_ShouldReturnRequestedSubset_WhenEdgesExists()
	{
		// Arrange
		Graph graph = CreateGraph();
		var edgeId1 = new EdgeId(Guid.NewGuid());
		var edgeId2 = new EdgeId(Guid.NewGuid());
		graph.AddEdges(new HashSet<EdgeByNode>
		{
			new EdgeByNode
			{
				Id = edgeId1,
				FromNodeId = new NodeId(Guid.NewGuid()),
				ToNodeId = new NodeId(Guid.NewGuid())
			},
			new EdgeByNode
			{
				Id = edgeId2,
				FromNodeId = new NodeId(Guid.NewGuid()),
				ToNodeId = new NodeId(Guid.NewGuid())
			}
		});

		// Act
		IReadOnlyCollection<EdgeByNode> edges = graph.GetEdges(new HashSet<EdgeId> { edgeId2 });

		// Assert
		edges.Should().HaveCount(1);
		edges.Single().Id.Should().Be(edgeId2);
	}

	[Fact]
	public void GetEdges_ShouldThrowEdgeNotFoundException_WhenEdgesDoNotExist()
	{
		// Arrange
		Graph graph = CreateGraph();
		var existingEdgeId = new EdgeId(Guid.NewGuid());
		var missingEdgeId = new EdgeId(Guid.NewGuid());
		graph.AddEdges(new HashSet<EdgeByNode>
		{
			new()
			{
				Id = existingEdgeId, FromNodeId = new NodeId(Guid.NewGuid()), ToNodeId = new NodeId(Guid.NewGuid())
			}
		});

		// Act
		Action act = () => graph.GetEdges(new HashSet<EdgeId> { missingEdgeId });

		// Assert
		act.Should().Throw<EdgeNotFoundException>();
	}

	private static Graph CreateGraph()
	{
		return new Graph(LanguageDictionaryProvider.Instance);
	}
}
