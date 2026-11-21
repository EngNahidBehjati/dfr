using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecution.Business;

public sealed class NodeExecutionRevokerTests
{
	private readonly INodeExecutionStateUpdater _stateUpdater;
	private readonly INodeExecutionStateProvider _stateProvider;
	private readonly INodeCanceller _nodeCanceller;
	private readonly IDataFlowCancellationNodeNextState _nodeNextState;

	private readonly NodeExecutionRevoker _sut;

	public NodeExecutionRevokerTests()
	{
		_stateUpdater = Substitute.For<INodeExecutionStateUpdater>();
		_stateProvider = Substitute.For<INodeExecutionStateProvider>();
		_nodeCanceller = Substitute.For<INodeCanceller>();
		_nodeNextState = Substitute.For<IDataFlowCancellationNodeNextState>();

		_sut = new NodeExecutionRevoker(_stateUpdater, _stateProvider, _nodeCanceller);
	}

	[Fact]
	public async Task RevokeNodeExecution_ShouldCancelNode_WhenStateIsRunning()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var node = new Node { Id = new NodeId(Guid.NewGuid()) };

		_stateProvider.GetNodeExecutionState(executionId, node.Id)
			.Returns(NodeExecutionState.Running);

		_nodeNextState.TryGetNodeNextState(NodeExecutionState.Running, out _)
			.Returns(false);

		// Act
		await _sut.RevokeNodeExecution(executionId, [node.Id], _nodeNextState, CancellationToken.None);

		// Assert
		await _nodeCanceller.Received(1)
			.CancelNodeAsync(executionId, node.Id, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task RevokeNodeExecution_ShouldChangeState_WhenNextStateExists()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var node = new Node { Id = new NodeId(Guid.NewGuid()) };

		_stateProvider.GetNodeExecutionState(executionId, node.Id)
			.Returns(NodeExecutionState.Pending);

		_nodeNextState.TryGetNodeNextState(NodeExecutionState.Pending, out Arg.Any<NodeExecutionState>())
			.Returns(x =>
			{
				x[1] = NodeExecutionState.Canceled;
				return true;
			});

		// Act
		await _sut.RevokeNodeExecution(executionId, [node.Id], _nodeNextState, CancellationToken.None);

		// Assert
		_stateUpdater.Received(1).ChangeNodeExecutionState(executionId, node.Id, NodeExecutionState.Canceled);
	}

	[Fact]
	public async Task RevokeNodeExecution_ShouldNotChangeState_WhenNextStateDoesNotExist()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var node = new Node { Id = new NodeId(Guid.NewGuid()) };

		_stateProvider.GetNodeExecutionState(executionId, node.Id)
			.Returns(NodeExecutionState.Succeeded);

		_nodeNextState.TryGetNodeNextState(NodeExecutionState.Succeeded, out _)
			.Returns(false);

		// Act
		await _sut.RevokeNodeExecution(executionId, [node.Id], _nodeNextState, CancellationToken.None);

		// Assert
		_stateUpdater.DidNotReceive().ChangeNodeExecutionState(
				Arg.Any<DataFlowExecutionId>(), Arg.Any<NodeId>(), Arg.Any<NodeExecutionState>());
	}
}
