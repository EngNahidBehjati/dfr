using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Business;
using DataFlowRunner.NodeExecutionStates.Exceptions;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;

namespace DataFlowRunner.Tests.NodeExecutionStates.Business;

public sealed class NodeExecutionStateMachineTests
{
	private readonly NodeExecutionStateMachine _sut;

	public NodeExecutionStateMachineTests()
	{
		_sut = new NodeExecutionStateMachine(LanguageDictionaryProvider.Instance);
	}

	[Fact]
	public void CanTransition_ShouldThrowInvalidNodeExecutionStateException_WhenCurrentStateIsInvalid()
	{
		// Arrange
		const NodeExecutionState invalidState = (NodeExecutionState)999;

		// Act
		Func<bool> act = () => _sut.CanTransition(invalidState, NodeExecutionState.Idle);

		// Assert
		act.Should().Throw<InvalidNodeExecutionStateException>();
	}

	public static readonly TheoryData<int> AllNodeExecutionStates =
		new(Enum.GetValues<NodeExecutionState>().Select(state => (int)state));

	[Theory]
	[MemberData(nameof(AllNodeExecutionStates))]
	public void CanTransition_ShouldReturnFalse_WhenTransitioningToSameState(int stateValue)
	{
		// Arrange
		var state = (NodeExecutionState)stateValue;

		// Act
		var result = _sut.CanTransition(state, state);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void CanTransition_ShouldReturnTrue_WhenTransitionIsAllowed()
	{
		// Arrange
		const NodeExecutionState current = NodeExecutionState.Idle;
		const NodeExecutionState next = NodeExecutionState.Pending;

		// Act
		var result = _sut.CanTransition(current, next);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void CanTransition_ShouldReturnFalse_WhenTransitionIsNotAllowed()
	{
		// Arrange
		const NodeExecutionState current = NodeExecutionState.Idle;
		const NodeExecutionState next = NodeExecutionState.Succeeded;

		// Act
		var result = _sut.CanTransition(current, next);

		// Assert
		result.Should().BeFalse();
	}
}
