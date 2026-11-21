using DataFlowRunner.NodeExecutionStates.Business;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;

namespace DataFlowRunner.Tests.NodeExecutionStates.Business;

public class NodeExecutionStateClassifierTests
{
	private readonly NodeExecutionStateClassifier _sut = new();

	[Theory]
	[InlineData((int)NodeExecutionState.Pending)]
	[InlineData((int)NodeExecutionState.Running)]
	[InlineData((int)NodeExecutionState.Canceling)]
	public void IsExecuting_ShouldReturnTrue_WhenStateIsExecuting(int state)
	{
		// Arrange
		var executionState = (NodeExecutionState)state;

		// Act
		var result = _sut.IsExecuting(executionState);

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData((int)NodeExecutionState.Idle)]
	[InlineData((int)NodeExecutionState.Succeeded)]
	[InlineData((int)NodeExecutionState.Failed)]
	[InlineData((int)NodeExecutionState.Canceled)]
	public void IsExecuting_ShouldReturnFalse_WhenStateIsNotExecuting(int state)
	{
		// Arrange
		var executionState = (NodeExecutionState)state;

		// Act
		var result = _sut.IsExecuting(executionState);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void IsExecuting_ShouldThrowArgumentOutOfRangeException_WhenStateIsInvalid()
	{
		// Arrange
		const NodeExecutionState invalidState = (NodeExecutionState)999;

		// Act
		Action act = () => _sut.IsExecuting(invalidState);

		// Assert
		act.Should().Throw<ArgumentOutOfRangeException>();
	}


	[Theory]
	[InlineData((int)NodeExecutionState.Canceled)]
	[InlineData((int)NodeExecutionState.Failed)]
	[InlineData((int)NodeExecutionState.Idle)]
	public void IsRunnable_ShouldReturnTrue_WhenStateIsRunnable(int state)
	{
		// Arrange
		var executionState = (NodeExecutionState)state;

		// Act
		var result = _sut.IsRunnable(executionState);

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData((int)NodeExecutionState.Running)]
	[InlineData((int)NodeExecutionState.Canceling)]
	[InlineData((int)NodeExecutionState.Succeeded)]
	[InlineData((int)NodeExecutionState.Pending)]
	public void IsRunnable_ShouldReturnFalse_WhenStateIsNotRunnable(int state)
	{
		// Arrange
		var executionState = (NodeExecutionState)state;

		// Act
		var result = _sut.IsRunnable(executionState);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void IsRunnable_ShouldThrowArgumentOutOfRangeException_WhenStateIsInvalid()
	{
		// Arrange
		const NodeExecutionState invalidState = (NodeExecutionState)999;

		// Act
		Action act = () => _sut.IsRunnable(invalidState);

		// Assert
		act.Should().Throw<ArgumentOutOfRangeException>();
	}
}
