using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Business;
using DataFlowRunner.NodeExecutionStates.DataTransferObjects;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.NodeExecutionStates.Business;

public sealed class NodeExecutionStatesDtoProviderTests
{
	private readonly INodeExecutionStateProvider _nodeExecutionStateProvider;
	private readonly INodeExecutionStateMapper _nodeExecutionStateMapper;
	private readonly INodeExecutionTimingStore _nodeExecutionTimingStore;
	private readonly INodeExecutionDurationResolver _nodeExecutionDurationResolver;
	private readonly NodeExecutionStatesDtoProvider _sut;

	public NodeExecutionStatesDtoProviderTests()
	{
		_nodeExecutionStateProvider = Substitute.For<INodeExecutionStateProvider>();
		_nodeExecutionStateMapper = Substitute.For<INodeExecutionStateMapper>();
		_nodeExecutionTimingStore = Substitute.For<INodeExecutionTimingStore>();
		_nodeExecutionDurationResolver = Substitute.For<INodeExecutionDurationResolver>();
		_sut = new NodeExecutionStatesDtoProvider(_nodeExecutionStateProvider,
			_nodeExecutionStateMapper, _nodeExecutionTimingStore, _nodeExecutionDurationResolver);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenNodeExecutionStateProviderIsNull()
	{
		// Arrange
		INodeExecutionStateProvider stateProvider = null!;

		// Act
		Action act = () => _ = new NodeExecutionStatesDtoProvider(stateProvider, _nodeExecutionStateMapper,
			_nodeExecutionTimingStore, _nodeExecutionDurationResolver);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("nodeExecutionStateProvider");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenNodeExecutionStateMapperIsNull()
	{
		// Arrange
		INodeExecutionStateMapper mapper = null!;

		// Act
		Action act = () => _ = new NodeExecutionStatesDtoProvider(_nodeExecutionStateProvider, mapper,
			_nodeExecutionTimingStore, _nodeExecutionDurationResolver);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("nodeExecutionStateMapper");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenNodeExecutionTimeStoreIsNull()
	{
		// Arrange
		INodeExecutionTimingStore nodeExecutionTimingStore = null!;

		// Act
		Action act = () => _ = new NodeExecutionStatesDtoProvider(_nodeExecutionStateProvider,
			_nodeExecutionStateMapper, nodeExecutionTimingStore, _nodeExecutionDurationResolver);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("nodeExecutionTimingStore");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenNodeExecutionDurationResolverIsNull()
	{
		// Arrange
		INodeExecutionDurationResolver nodeExecutionDurationResolver = null!;

		// Act
		Action act = () => _ = new NodeExecutionStatesDtoProvider(_nodeExecutionStateProvider,
			_nodeExecutionStateMapper, _nodeExecutionTimingStore, nodeExecutionDurationResolver);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("nodeExecutionDurationResolver");
	}

	[Fact]
	public async Task GetAllNodeExecutionStatesAsync_ShouldReturnEmptyList_WhenNoStatesExist()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		_nodeExecutionStateProvider
			.GetAllNodeExecutionStates(new DataFlowExecutionId(executionId))
			.Returns(new Dictionary<NodeId, NodeExecutionState>());

		// Act
		DataFlowNodeExecutionStatesDto result = await _sut.GetAllNodeExecutionStatesAsync(executionId, CancellationToken.None);

		// Assert
		result.NodeExecutionStates.Should().BeEmpty();
		_nodeExecutionStateMapper.DidNotReceive().NodeExecutionStateToDto(Arg.Any<NodeExecutionState>());
	}

	[Fact]
	public async Task GetAllNodeExecutionStatesDtoAsync_ShouldReturnAllStates_WhenStatesExist()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId1 = new NodeId(Guid.NewGuid());
		var nodeId2 = new NodeId(Guid.NewGuid());
		var nodeId3 = new NodeId(Guid.NewGuid());

		var statesById = new Dictionary<NodeId, NodeExecutionState>
		{
			{ nodeId1, NodeExecutionState.Idle },
			{ nodeId2, NodeExecutionState.Running },
			{ nodeId3, NodeExecutionState.Succeeded }
		};

		const string startTime1 = "2026-02-14T05:52:29.0017884+00:00";
		const string endTime1 = "2026-02-14T05:53:09.0017884+00:00";

		var executionTime1 = new NodeExecutionTiming
		{
			StartedAt = DateTimeOffset.Parse(startTime1),
			CompletedAt = DateTimeOffset.Parse(endTime1)
		};
		var executionTime2 = new NodeExecutionTiming();
		var executionTime3 = new NodeExecutionTiming();
		const int durationMilliseconds = 40000;

		_nodeExecutionDurationResolver.ResolveDurationMilliseconds(Arg.Is<NodeExecutionTiming>(timing =>
			timing.StartedAt == executionTime1.StartedAt && timing.CompletedAt == executionTime1.CompletedAt))
			.Returns(durationMilliseconds);

		_nodeExecutionStateProvider.GetAllNodeExecutionStates(dataFlowExecutionId)
			.Returns(statesById);

		_nodeExecutionStateMapper.NodeExecutionStateToDto(NodeExecutionState.Idle)
			.Returns(NodeExecutionStateDto.Idle);
		_nodeExecutionStateMapper.NodeExecutionStateToDto(NodeExecutionState.Running)
			.Returns(NodeExecutionStateDto.Running);
		_nodeExecutionStateMapper.NodeExecutionStateToDto(NodeExecutionState.Succeeded)
			.Returns(NodeExecutionStateDto.Succeeded);

		_nodeExecutionTimingStore.GetNodeExecutionTime(dataFlowExecutionId, nodeId1).Returns(executionTime1);
		_nodeExecutionTimingStore.GetNodeExecutionTime(dataFlowExecutionId, nodeId2).Returns(executionTime2);
		_nodeExecutionTimingStore.GetNodeExecutionTime(dataFlowExecutionId, nodeId3).Returns(executionTime3);

		var expectedDtoStates = new List<NodeExecutionStateByIdDto>
		{
			new()
			{
				NodeId = nodeId1.Value,
				NodeExecutionStateDto = NodeExecutionStateDto.Idle,
				DurationMilliseconds = durationMilliseconds
			},
			new()
			{
				NodeId = nodeId2.Value,
				NodeExecutionStateDto = NodeExecutionStateDto.Running
			},
			new()
			{
				NodeId = nodeId3.Value,
				NodeExecutionStateDto = NodeExecutionStateDto.Succeeded
			}
		};

		// Act
		DataFlowNodeExecutionStatesDto result = await _sut.GetAllNodeExecutionStatesAsync(
			dataFlowExecutionId.Value, CancellationToken.None);

		// Assert
		result.NodeExecutionStates.Should().BeEquivalentTo(expectedDtoStates);
		_nodeExecutionStateMapper.Received(1).NodeExecutionStateToDto(NodeExecutionState.Idle);
		_nodeExecutionStateMapper.Received(1).NodeExecutionStateToDto(NodeExecutionState.Running);
		_nodeExecutionStateMapper.Received(1).NodeExecutionStateToDto(NodeExecutionState.Succeeded);
	}
}
