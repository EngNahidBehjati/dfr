using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;

namespace DataFlowRunner.Tests.DataFlowExecution.Business;

public class DataFlowCancellationNodeNextStateTests
{
	private readonly DataFlowCancellationNodeNextState _sut;

	public DataFlowCancellationNodeNextStateTests()
	{
		_sut = new DataFlowCancellationNodeNextState();
	}

	[Fact]
	public void TryGetNodeNextState_ShouldReturnCancelingState_WhenNodeStateIsRunning()
	{
		// Arrange
		const NodeExecutionState currentState = NodeExecutionState.Running;

		// Act
		var result = _sut.TryGetNodeNextState(currentState, out NodeExecutionState nextState);

		// Assert
		result.Should().BeTrue();
		nextState.Should().Be(NodeExecutionState.Canceling);
	}

	[Fact]
	public void TryGetNodeNextState_ShouldReturnIdleState_WhenNodeStateIsPending()
	{
		// Arrange
		const NodeExecutionState currentState = NodeExecutionState.Pending;

		// Act
		var result = _sut.TryGetNodeNextState(currentState, out NodeExecutionState nextState);

		// Assert
		result.Should().BeTrue();
		nextState.Should().Be(NodeExecutionState.Idle);
	}

	[Theory]
	[InlineData((int)NodeExecutionState.Idle)]
	[InlineData((int)NodeExecutionState.Canceling)]
	[InlineData((int)NodeExecutionState.Canceled)]
	[InlineData((int)NodeExecutionState.Failed)]
	[InlineData((int)NodeExecutionState.Succeeded)]
	public void TryGetNodeNextState_ShouldReturnFalse_WhenNodeStateDoesNotRequireChange(int stateValue)
	{
		// Arrange
		var currentState = (NodeExecutionState)stateValue;

		// Act
		var result = _sut.TryGetNodeNextState(currentState, out NodeExecutionState nextState);

		// Assert
		result.Should().BeFalse();
		nextState.Should().Be(0);
	}
}
