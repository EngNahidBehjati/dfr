using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Business;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Business;
using DataFlowRunner.Language.LanguageDictionary;
using FluentAssertions;
using NSubstitute;

// ReSharper disable CollectionNeverUpdated.Local

namespace DataFlowRunner.Tests.GraphTraversal.Business;

public sealed class SubGraphCreatorTests
{
	private readonly IGraphFactory _graphFactory;
	private readonly SubGraphCreator _sut;

	public SubGraphCreatorTests()
	{
		_graphFactory = new GraphFactory(Substitute.For<ILanguageDictionaryProvider>());
		_sut = new SubGraphCreator(_graphFactory);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenGraphFactoryIsNull()
	{
		// Arrange
		IGraphFactory graphFactory = null!;

		// Act
		Action act = () => _ = new SubGraphCreator(graphFactory);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("graphFactory");
	}

	[Fact]
	public void CreateSubGraphFromNodes_ShouldReturnSubgraphWithGivenNodesOnly_WhenSourceHasMoreNodes()
	{
		// Arrange
		IGraph source = _graphFactory.CreateGraph();
		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());
		var nodeC = new NodeId(Guid.NewGuid());
		source.AddNodes(new List<NodeId> { nodeA, nodeB, nodeC });
		var subgraphNodeIds = new List<NodeId> { nodeA, nodeC };

		// Act
		IReadOnlyGraph result = _sut.CreateSubGraphFromNodes(source, subgraphNodeIds);

		// Assert
		var resultNodeIds = result.GetAllNodes().Select(n => n.Id).ToHashSet();
		resultNodeIds.Should().BeEquivalentTo([nodeA, nodeC]);
		result.GetAllEdges().Should().BeEmpty();
	}

	[Fact]
	public void CreateSubGraphFromNodes_ShouldIncludeOnlyEdgesBetweenSubgraphNodes_WhenSourceHasEdges()
	{
		// Arrange
		IGraph source = _graphFactory.CreateGraph();
		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());
		var nodeC = new NodeId(Guid.NewGuid());
		source.AddNodes(new List<NodeId> { nodeA, nodeB, nodeC });

		var edgeAB = new EdgeByNode
		{
			Id = new EdgeId(Guid.NewGuid()),
			FromNodeId = nodeA,
			ToNodeId = nodeB
		};
		var edgeBC = new EdgeByNode
		{
			Id = new EdgeId(Guid.NewGuid()),
			FromNodeId = nodeB,
			ToNodeId = nodeC
		};
		source.AddEdges(new List<EdgeByNode> { edgeAB, edgeBC });
		var subgraphNodeIds = new List<NodeId> { nodeA, nodeB };

		// Act
		IReadOnlyGraph result = _sut.CreateSubGraphFromNodes(source, subgraphNodeIds);

		// Assert
		result.GetAllNodes().Select(n => n.Id).Should().BeEquivalentTo([nodeA, nodeB]);
		result.GetAllEdges().Should().BeEquivalentTo([edgeAB]);
	}

	[Fact]
	public void CreateSubGraphFromNodes_ShouldReturnEmptySubgraph_WhenNodeIdsEmpty()
	{
		// Arrange
		IGraph source = _graphFactory.CreateGraph();
		var nodeA = new NodeId(Guid.NewGuid());
		source.AddNodes(new List<NodeId> { nodeA });
		List<NodeId> subgraphNodeIds = [];

		// Act
		IReadOnlyGraph result = _sut.CreateSubGraphFromNodes(source, subgraphNodeIds);

		// Assert
		result.GetAllNodes().Should().BeEmpty();
		result.GetAllEdges().Should().BeEmpty();
	}
}
