using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Controllers;
using DataFlowRunner.NodeExecutionStates.DataTransferObjects;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.NodeExecutionStates.Controllers;

public sealed class DataFlowExecutionStateControllerTests
{
	private readonly INodeExecutionStatesDtoProvider _nodeExecutionStatesDtoProvider;
	private readonly IDataFlowExecutionStatusDtoProvider _dataFlowExecutionStatusDtoProvider;
	private readonly DataFlowExecutionStateController _sut;

	public DataFlowExecutionStateControllerTests()
	{
		_nodeExecutionStatesDtoProvider = Substitute.For<INodeExecutionStatesDtoProvider>();
		_dataFlowExecutionStatusDtoProvider = Substitute.For<IDataFlowExecutionStatusDtoProvider>();
		_sut = new
			DataFlowExecutionStateController(_nodeExecutionStatesDtoProvider, _dataFlowExecutionStatusDtoProvider);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenNodeExecutionStatesDtoProviderIsNull()
	{
		// Arrange
		INodeExecutionStatesDtoProvider nodeExecutionStatesDtoProvider = null!;

		// Act
		Action act = () =>
		{
			_ = new DataFlowExecutionStateController(nodeExecutionStatesDtoProvider,
				_dataFlowExecutionStatusDtoProvider);
		};

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("nodeExecutionStatesDtoProvider");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowExecutionStatusDtoProviderIsNull()
	{
		// Arrange
		IDataFlowExecutionStatusDtoProvider dataFlowExecutionStatusDtoProvider = null!;

		// Act
		Action act = () =>
		{
			_ = new DataFlowExecutionStateController(_nodeExecutionStatesDtoProvider,
				dataFlowExecutionStatusDtoProvider);
		};

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("dataFlowExecutionStatusDtoProvider");
	}

	[Fact]
	public async Task GetNodeStates_ShouldCallProviderAndReturnDto_Whenever()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var expectedDto = new DataFlowNodeExecutionStatesDto(
		[
			new NodeExecutionStateByIdDto
			{
				NodeId = Guid.NewGuid(),
				NodeExecutionStateDto = NodeExecutionStateDto.Idle
			},
			new NodeExecutionStateByIdDto
			{
				NodeId = Guid.NewGuid(),
				NodeExecutionStateDto = NodeExecutionStateDto.Running
			}
		]);

		_nodeExecutionStatesDtoProvider
			.GetAllNodeExecutionStatesAsync(executionId, CancellationToken.None)
			.Returns(expectedDto);

		// Act
		DataFlowNodeExecutionStatesDto result = await _sut.GetNodeStates(executionId, CancellationToken.None);

		// Assert
		result.Should().Be(expectedDto);
	}

	[Fact]
	public async Task GetExecutionState_ShouldCallProviderAndReturnDataFlowExecutionStatusDto_Whenever()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		List<NodeExecutionStateByIdDto> expectedNodeStates =
		[
			new()
			{
				NodeId = Guid.NewGuid(),
				NodeExecutionStateDto = NodeExecutionStateDto.Idle
			},
			new()
			{
				NodeId = Guid.NewGuid(),
				NodeExecutionStateDto = NodeExecutionStateDto.Running
			}
		];

		var expectedDataFlowExecutionStatus = new DataFlowExecutionStatusDto()
		{
			ExecutionStatus = new ExecutionStatusDto { Failed = false, Message = null },
			NodeExecutionStates = expectedNodeStates
		};

		_dataFlowExecutionStatusDtoProvider
			.GetDataFlowExecutionStatusAsync(executionId, CancellationToken.None)
			.Returns(expectedDataFlowExecutionStatus);

		// Act
		DataFlowExecutionStatusDto result = await _sut.GetExecutionState(executionId, CancellationToken.None);

		// Assert
		result.Should().Be(expectedDataFlowExecutionStatus);
	}
}
