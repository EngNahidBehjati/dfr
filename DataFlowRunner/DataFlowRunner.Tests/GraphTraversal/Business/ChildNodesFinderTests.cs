using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Business;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.GraphTraversal.Business;

public sealed class ChildNodesFinderTests
{
	private readonly ChildNodesFinder _sut = new();

	[Fact]
	public void GetChildNodeIds_ShouldThrowArgumentNullException_WhenGraphIsNull()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());

		// Act
		Action act = () => _sut.GetChildNodeIds(nodeId, null!);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void GetChildNodeIds_ShouldReturnChildNodes_WhenNodeHaveChildNodes()
	{
		// Arrange
		var sourceNodeId = new NodeId(Guid.NewGuid());
		var targetNodeId1 = new NodeId(Guid.NewGuid());
		var targetNodeId2 = new NodeId(Guid.NewGuid());

		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		graph.GetAllEdges().Returns(new HashSet<EdgeByNode>
		{
			new()
			{
				Id = new EdgeId(Guid.NewGuid()),
				FromNodeId = sourceNodeId,
				ToNodeId = targetNodeId1
			},
			new()
			{
				Id = new EdgeId(Guid.NewGuid()),
				FromNodeId = sourceNodeId,
				ToNodeId = targetNodeId2
			},
			new()
			{
				Id = new EdgeId(Guid.NewGuid()),
				FromNodeId = new NodeId(Guid.NewGuid()),
				ToNodeId = new NodeId(Guid.NewGuid())
			}
		});

		// Act
		IReadOnlySet<NodeId> result = _sut.GetChildNodeIds(sourceNodeId, graph);

		// Assert
		result.Should().BeEquivalentTo([targetNodeId1, targetNodeId2]);
	}

	[Fact]
	public void GetChildNodeIds_ShouldReturnEmpty_WhenNodeHasNoChildNodes()
	{
		// Arrange
		var sourceNodeId = new NodeId(Guid.NewGuid());

		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		graph.GetAllEdges().Returns(new HashSet<EdgeByNode>
		{
			new()
			{
				Id = new EdgeId(Guid.NewGuid()),
				FromNodeId = new NodeId(Guid.NewGuid()),
				ToNodeId = new NodeId(Guid.NewGuid())
			}
		});

		// Act
		IReadOnlySet<NodeId> result = _sut.GetChildNodeIds(sourceNodeId, graph);

		// Assert
		result.Should().BeEmpty();
	}
}
