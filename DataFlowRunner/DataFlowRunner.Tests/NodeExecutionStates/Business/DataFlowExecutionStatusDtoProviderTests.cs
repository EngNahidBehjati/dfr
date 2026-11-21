using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Business;
using DataFlowRunner.NodeExecutionStates.DataTransferObjects;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.NodeExecutionStates.Business;

public sealed class DataFlowExecutionStatusDtoProviderTests
{
	private readonly INodeExecutionStatesDtoProvider _nodeExecutionStatesDtoProvider;
	private readonly IDataFlowExecutionStatusStore _dataFlowExecutionStatusStore;
	private readonly DataFlowExecutionStatusDtoProvider _sut;

	public DataFlowExecutionStatusDtoProviderTests()
	{
		_nodeExecutionStatesDtoProvider = Substitute.For<INodeExecutionStatesDtoProvider>();
		_dataFlowExecutionStatusStore = Substitute.For<IDataFlowExecutionStatusStore>();
		_sut = new DataFlowExecutionStatusDtoProvider(_nodeExecutionStatesDtoProvider, _dataFlowExecutionStatusStore);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenNodeExecutionStatesDtoProviderIsNull()
	{
		// Arrange
		INodeExecutionStatesDtoProvider nodeExecutionStatesDtoProvider = null!;

		// Act
		Action act = () => _ = new DataFlowExecutionStatusDtoProvider(nodeExecutionStatesDtoProvider,
			_dataFlowExecutionStatusStore);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowExecutionStatusStoreIsNull()
	{
		// Arrange
		IDataFlowExecutionStatusStore dataFlowExecutionStatusStore = null!;

		// Act
		Action act = () => _ = new DataFlowExecutionStatusDtoProvider(_nodeExecutionStatesDtoProvider,
			dataFlowExecutionStatusStore);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public async Task GetDataFlowExecutionStatusAsync_ShouldReturnDataFlowExecutionStatus_Whenever()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		var nodeStates = new List<NodeExecutionStateByIdDto>
		{
			new()
			{
				NodeId = Guid.NewGuid(),
				DurationMilliseconds = 10,
				NodeExecutionStateDto = NodeExecutionStateDto.Idle
			},
			new()
			{
				NodeId = Guid.NewGuid(),
				DurationMilliseconds = null,
				NodeExecutionStateDto = NodeExecutionStateDto.Failed
			}
		};
		var nodeExecutionStates = new DataFlowNodeExecutionStatesDto(nodeStates);
		DataFlowExecutionStatus dataFlowExecutionStatus = DataFlowExecutionStatus.DefaultStatus;
		var expected = new DataFlowExecutionStatusDto
		{
			ExecutionStatus = new ExecutionStatusDto
			{
				Failed = dataFlowExecutionStatus.Failed,
				Message = dataFlowExecutionStatus.FailureMessage
			},
			NodeExecutionStates = nodeExecutionStates.NodeExecutionStates
		};
		_dataFlowExecutionStatusStore.Get(new DataFlowExecutionId(dataFlowExecutionId))
			.Returns(dataFlowExecutionStatus);
		_nodeExecutionStatesDtoProvider
			.GetAllNodeExecutionStatesAsync(dataFlowExecutionId, Arg.Any<CancellationToken>())
			.Returns(nodeExecutionStates);

		// Act
		DataFlowExecutionStatusDto actual = await _sut.GetDataFlowExecutionStatusAsync(dataFlowExecutionId, CancellationToken.None);

		// Assert
		actual.Should().BeEquivalentTo(expected);
	}
}
