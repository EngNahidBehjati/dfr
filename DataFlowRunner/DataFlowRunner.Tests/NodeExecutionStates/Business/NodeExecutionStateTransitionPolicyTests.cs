using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Business;
using DataFlowRunner.NodeExecutionStates.Exceptions;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.NodeExecutionStates.Business;

public sealed class NodeExecutionStateTransitionPolicyTests
{
	private readonly INodeExecutionStateStore _executionStateStore;
	private readonly INodeExecutionStateMachine _stateMachine;
	private readonly NodeExecutionStateTransitionPolicy _sut;

	public NodeExecutionStateTransitionPolicyTests()
	{
		_executionStateStore = Substitute.For<INodeExecutionStateStore>();
		_stateMachine = Substitute.For<INodeExecutionStateMachine>();
		_sut = new NodeExecutionStateTransitionPolicy(
			_executionStateStore, _stateMachine, LanguageDictionaryProvider.Instance);
	}

	[Fact]
	public void EnsureTransitionAllowed_ShouldThrowNodeExecutionStateTransitionNotAllowedException_WhenTransitionIsNotAllowed()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		_executionStateStore.GetNodeExecutionState(executionId, nodeId).Returns(NodeExecutionState.Pending);
		_stateMachine.CanTransition(NodeExecutionState.Pending, NodeExecutionState.Idle).Returns(false);

		// Act
		Action act = () => _sut.EnsureTransitionAllowed(executionId, nodeId, NodeExecutionState.Idle);

		// Assert
		act.Should().Throw<NodeExecutionStateTransitionNotAllowedException>();
	}

	[Fact]
	public void EnsureTransitionAllowed_ShouldNotThrow_WhenTransitionIsAllowed()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		_executionStateStore.GetNodeExecutionState(executionId, nodeId).Returns(NodeExecutionState.Idle);
		_stateMachine.CanTransition(NodeExecutionState.Idle, NodeExecutionState.Pending).Returns(true);

		// Act
		Action act = () => _sut.EnsureTransitionAllowed(executionId, nodeId, NodeExecutionState.Pending);

		// Assert
		act.Should().NotThrow();
		_stateMachine.Received().CanTransition(NodeExecutionState.Idle, NodeExecutionState.Pending);
	}
}

