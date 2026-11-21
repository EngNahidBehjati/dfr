using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;
using DataFlowRunner.Tests.Common.FakeClasses;
using Microsoft.Extensions.DependencyInjection;

namespace DataFlowRunner.Tests.DataFlowExecution.Business;

public sealed class DataFlowDisposerTests
{
	private readonly INodeExecutionStateProvider _nodeExecutionStateProvider;
	private readonly INodeExecutionStateUpdater _nodeExecutionStateUpdater;
	private readonly IDataFlowExecutionCommandPublisher _executionCommandPublisher;
	private readonly INodeExecutorEngineGraphInitializationTracker _graphInitializationTracker;
	private readonly UserId _userId;
	private readonly DataFlowDisposer _sut;

	public DataFlowDisposerTests()
	{
		_nodeExecutionStateProvider = Substitute.For<INodeExecutionStateProvider>();
		_nodeExecutionStateUpdater = Substitute.For<INodeExecutionStateUpdater>();
		var actionLockExecutor = new FakeActionLockExecutor();
		_executionCommandPublisher = Substitute.For<IDataFlowExecutionCommandPublisher>();
		IDataFlowExecutionUserIdProvider dataFlowExecutionUserIdProvider = Substitute.For<IDataFlowExecutionUserIdProvider>();
		_graphInitializationTracker = Substitute.For<INodeExecutorEngineGraphInitializationTracker>();
		_userId = new UserId(Guid.NewGuid());
		dataFlowExecutionUserIdProvider
			.GetDataFlowExecutionUserId(Arg.Any<DataFlowExecutionId>())
			.Returns(_userId);

		_sut = new DataFlowDisposer(
			_nodeExecutionStateProvider,
			_nodeExecutionStateUpdater,
			actionLockExecutor,
			CreateServiceScopeFactory(_executionCommandPublisher),
			dataFlowExecutionUserIdProvider,
			_graphInitializationTracker);
	}

	[Fact]
	public async Task DisposeAsync_ShouldChangePendingNodesToIdle_WhenCalled()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var pendingNodeId = NodeId.New();
		StubNodeStates(dataFlowExecutionId, (pendingNodeId, NodeExecutionState.Pending));

		// Act
		await _sut.DisposeAsync(dataFlowExecutionId, CancellationToken.None);

		// Assert
		_nodeExecutionStateUpdater.Received(1)
			.ChangeNodeExecutionState(dataFlowExecutionId, pendingNodeId, NodeExecutionState.Idle);
	}

	[Fact]
	public async Task DisposeAsync_ShouldChangeSucceededNodesToIdle_WhenCalled()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var succeededNodeId = NodeId.New();
		StubNodeStates(dataFlowExecutionId, (succeededNodeId, NodeExecutionState.Succeeded));

		// Act
		await _sut.DisposeAsync(dataFlowExecutionId, CancellationToken.None);

		// Assert
		_nodeExecutionStateUpdater.Received(1)
			.ChangeNodeExecutionState(dataFlowExecutionId, succeededNodeId, NodeExecutionState.Idle);
	}

	[Fact]
	public async Task DisposeAsync_ShouldChangeRunningNodesToCanceled_WhenCalled()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var runningNodeId = NodeId.New();
		StubNodeStates(dataFlowExecutionId, (runningNodeId, NodeExecutionState.Running));

		// Act
		await _sut.DisposeAsync(dataFlowExecutionId, CancellationToken.None);

		// Assert
		_nodeExecutionStateUpdater.Received(1)
			.ChangeNodeExecutionState(dataFlowExecutionId, runningNodeId, NodeExecutionState.Canceled);
	}

	[Fact]
	public async Task DisposeAsync_ShouldChangeCancelingNodesToCanceled_WhenCalled()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var cancelingNodeId = NodeId.New();
		StubNodeStates(dataFlowExecutionId, (cancelingNodeId, NodeExecutionState.Canceling));

		// Act
		await _sut.DisposeAsync(dataFlowExecutionId, CancellationToken.None);

		// Assert
		_nodeExecutionStateUpdater.Received(1)
			.ChangeNodeExecutionState(dataFlowExecutionId, cancelingNodeId, NodeExecutionState.Canceled);
	}

	[Theory]
	[InlineData((int)NodeExecutionState.Idle)]
	[InlineData((int)NodeExecutionState.Failed)]
	[InlineData((int)NodeExecutionState.Canceled)]
	public async Task DisposeAsync_ShouldNotChangeNodeState_WhenStateDoesNotRequireTransition(int currentState)
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = NodeId.New();
		StubNodeStates(dataFlowExecutionId, (nodeId, (NodeExecutionState)currentState));

		// Act
		await _sut.DisposeAsync(dataFlowExecutionId, CancellationToken.None);

		// Assert
		_nodeExecutionStateUpdater.DidNotReceiveWithAnyArgs()
			.ChangeNodeExecutionState(default!, default!, default);
	}

	[Fact]
	public async Task DisposeAsync_ShouldPublishDisposeCommand_WhenCalled()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		StubNodeStates(dataFlowExecutionId);

		// Act
		await _sut.DisposeAsync(dataFlowExecutionId, CancellationToken.None);

		// Assert
		await _executionCommandPublisher.Received(1).PublishCommandAsync(
			Arg.Is<DisposeDataFlowExecutionCommand>(command =>
				command.GraphExecutionId == dataFlowExecutionId.Value &&
				command.UserId == _userId.Value),
			CancellationToken.None);
	}

	[Fact]
	public async Task DisposeAsync_ShouldUntrackDataFlowInitialization_WhenDisposeSucceeds()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		StubNodeStates(dataFlowExecutionId);

		// Act
		await _sut.DisposeAsync(dataFlowExecutionId, CancellationToken.None);

		// Assert
		_graphInitializationTracker.Received(1).UntrackDataFlowInitialization(dataFlowExecutionId);
	}

	private void StubNodeStates(
		DataFlowExecutionId dataFlowExecutionId,
		params (NodeId NodeId, NodeExecutionState State)[] states)
	{
		var nodeStates = states.ToDictionary(state => state.NodeId, state => state.State);
		_nodeExecutionStateProvider.GetAllNodeExecutionStates(dataFlowExecutionId).Returns(nodeStates);
	}

	private static IServiceScopeFactory CreateServiceScopeFactory(
		IDataFlowExecutionCommandPublisher executionCommandPublisher)
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSingleton(executionCommandPublisher);
		ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}
}
