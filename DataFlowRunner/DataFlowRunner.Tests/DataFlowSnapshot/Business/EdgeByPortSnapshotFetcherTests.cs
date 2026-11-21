using DataFlowRunner.Common.Authentication.Abstractions;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowSnapshot.Abstractions;
using DataFlowRunner.DataFlowSnapshot.Business;
using DataFlowRunner.DataFlowSnapshot.Models;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.PortModels;
using DataFlowRunner.NodePortConnections.Models;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowSnapshot.Business;

public sealed class EdgeByPortSnapshotFetcherTests
{
	private readonly IDataFlowTopologyClientApi _dataFlowTopologyClientApi;
	private readonly IEdgeByPortMapper _edgeByPortMapper;
	private readonly IAuthenticationTokenAccessor _authenticationTokenAccessor;
	private readonly EdgeByPortSnapshotFetcher _sut;

	public EdgeByPortSnapshotFetcherTests()
	{
		_dataFlowTopologyClientApi = Substitute.For<IDataFlowTopologyClientApi>();
		_authenticationTokenAccessor = Substitute.For<IAuthenticationTokenAccessor>();
		_edgeByPortMapper = Substitute.For<IEdgeByPortMapper>();
		_sut = new EdgeByPortSnapshotFetcher(_dataFlowTopologyClientApi, _authenticationTokenAccessor,
			_edgeByPortMapper);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowTopologyClientApiIsNull()
	{
		// Arrange
		IDataFlowTopologyClientApi api = null!;

		// Act
		Action act = () => { _ = new EdgeByPortSnapshotFetcher(api, _authenticationTokenAccessor, _edgeByPortMapper); };

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("dataFlowTopologyClientApi");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenAuthenticationTokenAccessorIsNull()
	{
		// Arrange
		IAuthenticationTokenAccessor tokenAccessor = null!;

		// Act
		Action act = () =>
		{
			_ = new EdgeByPortSnapshotFetcher(_dataFlowTopologyClientApi, tokenAccessor, _edgeByPortMapper);
		};

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("authenticationTokenAccessor");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenMapperIsNull()
	{
		// Arrange
		IEdgeByPortMapper mapper = null!;

		// Act
		Action act = () =>
		{
			_ = new EdgeByPortSnapshotFetcher(_dataFlowTopologyClientApi, _authenticationTokenAccessor, mapper);
		};

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("mapper");
	}


	[Fact]
	public async Task GetEdgeByPortAsync_ShouldCallApiAndMapEdges_Whenever()
	{
		// Arrange
		var graphId = Guid.NewGuid();
		var node1Id = Guid.NewGuid();
		var node2Id = Guid.NewGuid();
		var node1PortId = Guid.NewGuid();
		var node2PortId = Guid.NewGuid();
		var edgeId = Guid.NewGuid();

		var edges = new List<EdgeDto>
		{
			new(edgeId, node1PortId, node2PortId, node1Id, node2Id)
		};
		var nodes = new List<NodeDto>
		{
			new(node1Id, "type1", [new PortDto(node1PortId, PortDirectionDto.Out)], Version: 1),
			new(node2Id, "type2", [new PortDto(node2PortId, PortDirectionDto.In)], Version: 1)
		};
		var topology = new DataFlowTopologyDto(
			Topology: new TopologyDto(nodes, edges,
				new ProcessorInfoDto("PROCESSOR_LIST", Guid.NewGuid()),
				new ExecutionSettingsDto("PIPELINE_EXECUTOR", null)),
			DataFlowVersion: 42
		);
		var expectedEdgeByPorts = new List<EdgeByPort>
		{
			new()
			{
				Id = new EdgeId(edgeId),
				FromPortId = new OutputPortId(node1PortId),
				ToPortId = new InputPortId(node2PortId)
			}
		};

		_dataFlowTopologyClientApi.GetDataFlowTopologyAsync(graphId, Arg.Any<AuthenticationCookie>(),
				9, Arg.Any<CancellationToken>())
			.Returns(topology);
		_edgeByPortMapper.MapToEdgeByPorts(edges).Returns(expectedEdgeByPorts);

		// Act
		IReadOnlyList<EdgeByPort> result = await _sut.GetEdgeByPortAsync(graphId, CancellationToken.None, 9);

		// Assert
		result.Should().BeEquivalentTo(expectedEdgeByPorts);
	}
}
