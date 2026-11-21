using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphCore.Models.PortModels;
using DataFlowRunner.NodePortConnections.Abstract;
using DataFlowRunner.NodePortConnections.Business;
using DataFlowRunner.NodePortConnections.Models;
using DataFlowRunner.Tests.Common;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.NodePortConnections.Business;

public class NodeInputPortToUpstreamPortsResolverTests
{
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly IEdgeByPortContainer _edgeByPortContainer;
	private readonly NodeInputPortToUpstreamPortsResolver _sut;

	public NodeInputPortToUpstreamPortsResolverTests()
	{
		_dataFlowExecutionContextProvider = Substitute.For<IDataFlowExecutionContextProvider>();
		_edgeByPortContainer = Substitute.For<IEdgeByPortContainer>();
		_sut = new NodeInputPortToUpstreamPortsResolver(_dataFlowExecutionContextProvider, _edgeByPortContainer);
	}

	[Fact]
	public async Task GetNodeInputPortsToUpstreamPortsAsync_ShouldReturnNodeUpstreamPorts_Whenever()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());
		var otherNodeId = new NodeId(Guid.NewGuid());
		var executionId = new DataFlowExecutionId(Guid.NewGuid());

		var edge1 = new EdgeByNode { Id = new EdgeId(Guid.NewGuid()), FromNodeId = otherNodeId, ToNodeId = nodeId };
		var edge2 = new EdgeByNode { Id = new EdgeId(Guid.NewGuid()), FromNodeId = otherNodeId, ToNodeId = nodeId };
		var edge3 = new EdgeByNode { Id = new EdgeId(Guid.NewGuid()), FromNodeId = otherNodeId, ToNodeId = new NodeId(Guid.NewGuid()) };

		var allEdges = new HashSet<EdgeByNode> { edge1, edge2, edge3 };

		var port1 = new EdgeByPort
		{
			Id = edge1.Id,
			FromPortId = new OutputPortId(Guid.NewGuid()),
			ToPortId = new InputPortId(Guid.NewGuid())
		};

		var port2 = new EdgeByPort
		{
			Id = edge2.Id,
			FromPortId = new OutputPortId(Guid.NewGuid()),
			ToPortId = port1.ToPortId
		};

		IGraph graph = Substitute.For<IGraph>();
		graph.GetAllEdges().Returns(allEdges);

		var context = new DataFlowExecutionContext
		{
			Graph = graph,
			DataFlowExecutionId = executionId,
			DataFlowVersion = new DataFlowVersion(),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
		_dataFlowExecutionContextProvider.GetRunningDataFlowExecutionContextAsync(executionId, CancellationToken.None)
			.Returns(context);

		_edgeByPortContainer.GetEdgeByPort(edge1.Id).Returns(port1);
		_edgeByPortContainer.GetEdgeByPort(edge2.Id).Returns(port2);

		// Act
		IReadOnlyCollection<NodeInputPortToUpstreamPorts> result = await
			_sut.GetNodeInputPortsToUpstreamPortsAsync(executionId, nodeId, CancellationToken.None);

		// Assert
		result.Should().HaveCount(1);

		NodeInputPortToUpstreamPorts inputPortConnection = result.Single();
		inputPortConnection.InputPort.Should().Be(port1.ToPortId.Value);
		inputPortConnection.UpstreamPorts.Should().HaveCount(2);
		inputPortConnection.UpstreamPorts.Should().Contain([port1.FromPortId.Value, port2.FromPortId.Value]);
	}

	[Fact]
	public async Task GetNodeInputPortsToUpstreamPortsAsync_ShouldReturnEmptyList_WhenNodeHasNoIncomingEdges()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var otherNodeId = new NodeId(Guid.NewGuid());

		var edge = new EdgeByNode { Id = new EdgeId(Guid.NewGuid()), FromNodeId = otherNodeId, ToNodeId = otherNodeId };
		var allEdges = new HashSet<EdgeByNode> { edge };

		IGraph graph = Substitute.For<IGraph>();
		graph.GetAllEdges().Returns(allEdges);

		var context = new DataFlowExecutionContext() { Graph = graph, DataFlowExecutionId = executionId, DataFlowVersion = new DataFlowVersion() ,
			ProcessorInfo = DefaultProcessorInfoCreator.Create(), ExecutionSettings = DefaultExecutionSettingsCreator.Create()};
		_dataFlowExecutionContextProvider.GetRunningDataFlowExecutionContextAsync(executionId, CancellationToken.None)
			.Returns(context);

		// Act
		IReadOnlyCollection<NodeInputPortToUpstreamPorts> result = await
			_sut.GetNodeInputPortsToUpstreamPortsAsync(executionId, nodeId, CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}
}
