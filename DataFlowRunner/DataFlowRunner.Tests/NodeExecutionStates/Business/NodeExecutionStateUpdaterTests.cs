using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowSynchronization.Abstraction;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Business;
using DataFlowRunner.NodeExecutionStates.Models;
using DataFlowRunner.Tests.Common.FakeClasses;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.NodeExecutionStates.Business;

public sealed class NodeExecutionStateUpdaterTests
{
	private readonly INodeExecutionStateStore _executionStateStore;
	private readonly INodeExecutionStateTransitionPolicy _nodeExecutionStateTransitionPolicy;
	private readonly INodeExecutionTimeRecorder _nodeExecutionTimeRecorder;
	private readonly NodeExecutionStateUpdater _sut;

	public NodeExecutionStateUpdaterTests()
	{
		_executionStateStore = Substitute.For<INodeExecutionStateStore>();
		_nodeExecutionStateTransitionPolicy = Substitute.For<INodeExecutionStateTransitionPolicy>();
		_nodeExecutionTimeRecorder = Substitute.For<INodeExecutionTimeRecorder>();
		IDataFlowActionLockExecutor dataFlowActionLockExecutor = new FakeActionLockExecutor();

		_sut = new NodeExecutionStateUpdater(_executionStateStore, _nodeExecutionStateTransitionPolicy,
			_nodeExecutionTimeRecorder, dataFlowActionLockExecutor);
	}

	[Fact]
	public void ChangeNodeExecutionState_ShouldThrowInvalidOperationExceptionAndNotSetState_WhenTransitionIsNotAllowed()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		_nodeExecutionStateTransitionPolicy
			.When(x => x.EnsureTransitionAllowed(executionId, nodeId, NodeExecutionState.Idle))
			.Do(_ => throw new InvalidOperationException("Transition not permitted"));

		// Act
		Action act = () => _sut.ChangeNodeExecutionState(executionId, nodeId, NodeExecutionState.Idle);

		// Assert
		act.Should().Throw<InvalidOperationException>();
		_executionStateStore.DidNotReceive().SetNodeExecutionState(executionId, nodeId, NodeExecutionState.Idle);
	}

	[Fact]
	public void ChangeNodeExecutionState_ShouldUpdateStateAndCallExecutionTimeRecorder_WhenTransitionIsAllowed()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		_executionStateStore.GetNodeExecutionState(executionId, nodeId).Returns(NodeExecutionState.Idle);

		// Act
		_sut.ChangeNodeExecutionState(executionId, nodeId, NodeExecutionState.Pending);

		// Assert
		_nodeExecutionStateTransitionPolicy.Received()
			.EnsureTransitionAllowed(executionId, nodeId, NodeExecutionState.Pending);
		_executionStateStore.Received()
			.SetNodeExecutionState(executionId, nodeId, NodeExecutionState.Pending);
		_nodeExecutionTimeRecorder.Received(1)
			.RecordNodeExecutionTime(executionId, nodeId, NodeExecutionState.Idle, NodeExecutionState.Pending);
	}

	[Fact]
	public void TryAddNodeExecutionState_ShouldSetNodeStateToIdle_WhenNodeStateDoesNotExist()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		_executionStateStore.TryGetNodeExecutionState(executionId, nodeId, out Arg.Any<NodeExecutionState?>())
			.Returns(callInfo =>
			{
				callInfo[2] = null;
				return false;
			});

		// Act
		_sut.TryAddNodeExecutionState(executionId, nodeId);

		// Assert
		_executionStateStore.Received(1).SetNodeExecutionState(executionId, nodeId, NodeExecutionState.Idle);
	}

	[Fact]
	public void TryAddNodeExecutionState_ShouldNotSetNodeState_WhenNodeStateAlreadyExist()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		_executionStateStore.TryGetNodeExecutionState(executionId, nodeId, out Arg.Any<NodeExecutionState?>())
			.Returns(callInfo =>
			{
				callInfo[2] = NodeExecutionState.Succeeded;
				return true;
			});

		// Act
		_sut.TryAddNodeExecutionState(executionId, nodeId);

		// Assert
		_executionStateStore.DidNotReceive().SetNodeExecutionState(executionId, nodeId, Arg.Any<NodeExecutionState>());
	}

	[Fact]
	public void ResetNodeExecutionState_ShouldSetNodeStateAsIdle_Whenever()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());

		// Act
		_sut.ResetNodeExecutionState(dataFlowExecutionId, nodeId);

		// Assert
		_executionStateStore.Received(1).SetNodeExecutionState(dataFlowExecutionId, nodeId, NodeExecutionState.Idle);
	}
}
