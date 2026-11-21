using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;

namespace DataFlowRunner.Tests.DataFlowExecution.Business;

public class DataFlowInvalidationNodeNextStateTest
{
	private readonly DataFlowInvalidationNodeNextState _sut;

	public DataFlowInvalidationNodeNextStateTest()
	{
		_sut = new DataFlowInvalidationNodeNextState();
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

	[Theory]
	[InlineData((int)NodeExecutionState.Pending)]
	[InlineData((int)NodeExecutionState.Succeeded)]
	public void TryGetNodeNextState_ShouldReturnIdleState_WhenNodeStateIsPendingOrSucceeded(int stateValue)
	{
		// Arrange
		var currentState = (NodeExecutionState)stateValue;

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
