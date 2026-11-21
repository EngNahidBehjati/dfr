using DataFlowRunner.Common.Authentication.Abstractions;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowSnapshot.Abstractions;
using DataFlowRunner.DataFlowSnapshot.Business;
using DataFlowRunner.DataFlowSnapshot.Models;
using DataFlowRunner.GraphCore.Abstractions;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowSnapshot.Controllers;

public class DataFlowExecutionContextCreatorTests
{
	private readonly IDataFlowTopologyClientApi _clientApiMock;
	private readonly ITopologyMapper _mapperMock;
	private readonly AuthenticationCookie _cookie = new("test");
	private readonly DataFlowExecutionContextCreator _sut;

	public DataFlowExecutionContextCreatorTests()
	{
		_clientApiMock = Substitute.For<IDataFlowTopologyClientApi>();
		_mapperMock = Substitute.For<ITopologyMapper>();
		IAuthenticationTokenAccessor tokenAccessorMock = Substitute.For<IAuthenticationTokenAccessor>();
		tokenAccessorMock.GetAuthenticationCookie().Returns(_cookie);
		_sut = new DataFlowExecutionContextCreator(_clientApiMock, _mapperMock, tokenAccessorMock);
	}

	[Fact]
	public async Task GetDataFlowTopologyAsync_ShouldCallClientAndMapper_AndReturnGraph()
	{
		// Arrange
		var dataFlowId = Guid.NewGuid();
		const int version = 3;

		var processorId = Guid.NewGuid();
		var topology = new DataFlowTopologyDto(
			Topology:
			new TopologyDto(
				Nodes: [],
				Edges: [],
				new ProcessorInfoDto("PROCESSOR_LIST", processorId),
				new ExecutionSettingsDto("PIPELINE_EXECUTOR", null)
			),
			DataFlowVersion: 42
		);

		IGraph expectedGraph = Substitute.For<IGraph>();
		_clientApiMock
			.GetDataFlowTopologyAsync(dataFlowId, _cookie, version)
			.Returns(topology);

		_mapperMock
			.MapToGraph(dataFlowId, topology)
			.Returns(expectedGraph);

		// Act
		DataFlowExecutionContext result =
			await _sut.CreateDataFlowExecutionContextAsync(dataFlowId, CancellationToken.None, version);

		// Assert
		result.Graph.Should().Be(expectedGraph);
		result.ProcessorInfo.ProcessorType.Should().Be("PROCESSOR_LIST");
		result.ProcessorInfo.ProcessorId.Should().Be(processorId);
		result.ExecutionSettings.ExecutorType.Should().Be("PIPELINE_EXECUTOR");
		result.ExecutionSettings.ExecutorId.Should().BeNull();
		result.DataFlowVersion.Should().Be(new DataFlowVersion(42));
		result.DataFlowExecutionId.Should().Be(new DataFlowExecutionId(dataFlowId));

		await _clientApiMock.Received(1).GetDataFlowTopologyAsync(dataFlowId, _cookie, version);
		_mapperMock.Received(1).MapToGraph(dataFlowId, topology);
	}

	[Fact]
	public async Task GetDataFlowTopologyAsync_ShouldCallClientWithNullVersion_WhenVersionNotProvided()
	{
		// Arrange
		var dataFlowId = Guid.NewGuid();
		var topologyDto = new TopologyDto(
			Nodes: [],
			Edges: [],
			new ProcessorInfoDto("PROCESSOR_LIST", Guid.NewGuid()),
			new ExecutionSettingsDto("PIPELINE_EXECUTOR", null)
		);
		var topology = new DataFlowTopologyDto(
			Topology: topologyDto,
			DataFlowVersion: 1
		);

		IGraph expectedGraph = Substitute.For<IGraph>();
		_clientApiMock
			.GetDataFlowTopologyAsync(dataFlowId, _cookie)
			.Returns(topology);

		_mapperMock
			.MapToGraph(dataFlowId, topology)
			.Returns(expectedGraph);

		// Act
		DataFlowExecutionContext result =
			await _sut.CreateDataFlowExecutionContextAsync(dataFlowId, CancellationToken.None);

		// Assert
		result.Graph.Should().Be(expectedGraph);

		await _clientApiMock.Received(1).GetDataFlowTopologyAsync(dataFlowId, _cookie);
		_mapperMock.Received(1).MapToGraph(dataFlowId, topology);
	}
}
