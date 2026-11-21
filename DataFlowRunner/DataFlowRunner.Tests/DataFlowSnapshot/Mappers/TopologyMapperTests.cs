using DataFlowRunner.DataFlowSnapshot.Mappers;
using DataFlowRunner.DataFlowSnapshot.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowSnapshot.Mappers;

public class TopologyMapperTests
{
	private readonly IGraphFactory _graphFactory = Substitute.For<IGraphFactory>();
	private readonly IGraph _graph = Substitute.For<IGraph>();
	private readonly TopologyMapper _sut;

	public TopologyMapperTests()
	{
		_sut = new TopologyMapper(_graphFactory);
	}

	[Fact]
	public void MapToGraph_ShouldCreateGraphAndMapTopology()
	{
		// Arrange
		var graphId = Guid.NewGuid();
		var node1Id = Guid.NewGuid();
		var node2Id = Guid.NewGuid();
		var node1PortId = Guid.NewGuid();
		var node2PortId = Guid.NewGuid();
		var edgeId = Guid.NewGuid();

		var topologyDto = new TopologyDto(
			[
				new NodeDto(node1Id, "type1", [new PortDto(node1PortId, PortDirectionDto.Out)], Version: 1),
				new NodeDto(node2Id, "type2", [new PortDto(node2PortId, PortDirectionDto.In)], Version: 1)
			],
			[
				new EdgeDto(edgeId, node1PortId, node2PortId, node1Id, node2Id)
			],
			new ProcessorInfoDto("PROCESSOR_LIST", Guid.NewGuid()),
			new ExecutionSettingsDto("PIPELINE_EXECUTOR", null)
		);
		var topology = new DataFlowTopologyDto(
			Topology: topologyDto,
			DataFlowVersion: 42
		);

		var expectedNodes = new HashSet<NodeId>
		{
			new(node1Id),
			new(node2Id)
		};

		var expectedEdge = new EdgeByNode()
		{
			Id = new EdgeId(edgeId),
			FromNodeId = new NodeId(node1Id),
			ToNodeId = new NodeId(node2Id)
		};

		_graphFactory.CreateGraph().Returns(_graph);

		// Act
		IGraph result = _sut.MapToGraph(graphId, topology);

		// Assert
		result.Should().Be(_graph);

		_graphFactory.Received(1)
			.CreateGraph();

		_graph.Received(1)
			.AddNodes(Arg.Is<IReadOnlyCollection<NodeId>>(set =>
				set.Count == expectedNodes.Count && expectedNodes.All(set.Contains)));

		_graph.Received(1).AddEdges(Arg.Is<IReadOnlyCollection<EdgeByNode>>(edges =>
			edges.Count == 1 &&
			edges.Single().Id.Equals(expectedEdge.Id) &&
			edges.Single().ToNodeId.Equals(expectedEdge.ToNodeId) &&
			edges.Single().FromNodeId.Equals(expectedEdge.FromNodeId)));
	}
}
