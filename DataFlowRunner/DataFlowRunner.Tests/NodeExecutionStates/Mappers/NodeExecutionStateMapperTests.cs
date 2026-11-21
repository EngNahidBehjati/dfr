using DataFlowRunner.NodeExecutionStates.DataTransferObjects;
using DataFlowRunner.NodeExecutionStates.Mappers;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;

namespace DataFlowRunner.Tests.NodeExecutionStates.Mappers;

public sealed class NodeExecutionStateMapperTests
{
	private readonly NodeExecutionStateMapper _sut = new();

	[Theory]
	[InlineData((int)NodeExecutionState.Idle, NodeExecutionStateDto.Idle)]
	[InlineData((int)NodeExecutionState.Pending, NodeExecutionStateDto.Pending)]
	[InlineData((int)NodeExecutionState.Running, NodeExecutionStateDto.Running)]
	[InlineData((int)NodeExecutionState.Succeeded, NodeExecutionStateDto.Succeeded)]
	[InlineData((int)NodeExecutionState.Failed, NodeExecutionStateDto.Failed)]
	[InlineData((int)NodeExecutionState.Canceled, NodeExecutionStateDto.Canceled)]
	[InlineData((int)NodeExecutionState.Canceling, NodeExecutionStateDto.Canceling)]
	public void NodeExecutionStateToDto_ShouldMapToCorrectDto_WhenGivenExecutionState(
		int executionStateValue,
		NodeExecutionStateDto expectedDto)
	{
		// Arrange
		var executionState = (NodeExecutionState)executionStateValue;

		// Act
		NodeExecutionStateDto result = _sut.NodeExecutionStateToDto(executionState);

		// Assert
		result.Should().Be(expectedDto);
	}
}
