using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Business;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;
using DataFlowRunner.GraphTraversal.Business;
using DataFlowRunner.Language.LanguageDictionary;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.GraphExecute.Business;

public class UpstreamSubgraphDetectorTests
{
	private readonly DataFlowExecutionId _executionId = new(Guid.NewGuid());
	private readonly UpstreamSubgraphDetector _sut;
	private readonly ISubGraphCreator _subGraphCreator;

	public UpstreamSubgraphDetectorTests()
	{
		_subGraphCreator = Substitute.For<ISubGraphCreator>();
		_sut = new UpstreamSubgraphDetector(_subGraphCreator);
	}

	[Fact]
	public void DetectUpstreamSubgraph_ShouldReturnUpstreamSubgraph_WhenInputIsValid()
	{
		// Arrange
		IGraph graph = new GraphFactory(LanguageDictionaryProvider.Instance).CreateGraph();

		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());
		var nodeC = new NodeId(Guid.NewGuid());
		var nodeD = new NodeId(Guid.NewGuid());
		var nodeE = new NodeId(Guid.NewGuid());
		var nodeF = new NodeId(Guid.NewGuid());
		var disconnected = new NodeId(Guid.NewGuid());

		graph.AddNodes(new HashSet<NodeId>
		{
			nodeA,
			nodeB,
			nodeC,
			nodeD,
			nodeE,
			nodeF,
			disconnected
		});

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
		var edgeDB = new EdgeByNode
		{
			Id = new EdgeId(Guid.NewGuid()),
			FromNodeId = nodeD,
			ToNodeId = nodeB
		};
		var edgeED = new EdgeByNode
		{
			Id = new EdgeId(Guid.NewGuid()),
			FromNodeId = nodeE,
			ToNodeId = nodeD
		};
		var edgeBF = new EdgeByNode
		{
			Id = new EdgeId(Guid.NewGuid()),
			FromNodeId = nodeB,
			ToNodeId = nodeF
		};

		graph.AddEdges(new HashSet<EdgeByNode>
		{
			edgeAB,
			edgeBC,
			edgeDB,
			edgeED,
			edgeBF
		});

		var subgraphNodes = new HashSet<NodeId>
		{
			nodeA, nodeB, nodeC, nodeD, nodeE
		};
		IGraph expectedSubgraph = Substitute.For<IGraph>();
		_subGraphCreator.CreateSubGraphFromNodes(graph, Arg.Is<IReadOnlyCollection<NodeId>>(
				nodes => subgraphNodes.SetEquals(nodes)))
			.Returns(expectedSubgraph);

		// Act
		IReadOnlyGraph subgraph = _sut.DetectUpstreamSubgraph(graph, _executionId, nodeC);

		// Assert
		subgraph.Should().Be(expectedSubgraph);
	}

	[Fact]
	public void DetectUpstreamSubgraph_ShouldReturnGraphWithOnlyOneNode_WhenTargetHasNoAncestors()
	{
		// Arrange
		IGraph graph = new GraphFactory(LanguageDictionaryProvider.Instance).CreateGraph();
		var target = new NodeId(Guid.NewGuid());
		var unrelated = new NodeId(Guid.NewGuid());

		graph.AddNodes(new HashSet<NodeId> { target, unrelated });

		var subgraphNodes = new List<NodeId>
		{
			target
		};
		IGraph expectedSubgraph = Substitute.For<IGraph>();
		_subGraphCreator.CreateSubGraphFromNodes(graph, Arg.Is<IReadOnlyCollection<NodeId>>(
			nodes => nodes.SequenceEqual(subgraphNodes))).Returns(expectedSubgraph);

		// Act
		IReadOnlyGraph subgraph = _sut.DetectUpstreamSubgraph(graph, _executionId, target);

		// Assert
		subgraph.Should().Be(expectedSubgraph);
	}
}
