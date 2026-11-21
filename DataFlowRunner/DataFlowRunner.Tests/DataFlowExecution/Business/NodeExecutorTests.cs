using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.DataFlowNodeConfigs.Abstractions;
using DataFlowRunner.DataFlowNodeConfigs.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecution.Business;

public sealed class NodeExecutorTests
{
	private readonly INodeExecutionStateUpdater _nodeExecutionStateUpdater;
	private readonly IDataFlowExecutionCommandPublisher _executionCommandPublisher;
	private readonly IDataFlowNodeConfigsStore _dataFlowConfigsStore;
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly IDataFlowExecutionUserIdProvider _dataFlowExecutionUserIdProvider;
	private readonly IDataFlowDynamicParametersStore _dynamicParametersStore;
	private readonly NodeExecutor _sut;

	public NodeExecutorTests()
	{
		_dataFlowExecutionUserIdProvider = Substitute.For<IDataFlowExecutionUserIdProvider>();
		_nodeExecutionStateUpdater = Substitute.For<INodeExecutionStateUpdater>();
		_executionCommandPublisher = Substitute.For<IDataFlowExecutionCommandPublisher>();
		_serviceScopeFactory = CreateServiceScopeFactory(_executionCommandPublisher);
		_dataFlowConfigsStore = Substitute.For<IDataFlowNodeConfigsStore>();
		_dynamicParametersStore = Substitute.For<IDataFlowDynamicParametersStore>();
		_sut = new NodeExecutor(
			_nodeExecutionStateUpdater,
			_serviceScopeFactory,
			_dataFlowConfigsStore,
			_dataFlowExecutionUserIdProvider,
			_dynamicParametersStore);
	}

	private static IServiceScopeFactory CreateServiceScopeFactory(
		IDataFlowExecutionCommandPublisher executionCommandPublisher)
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSingleton(executionCommandPublisher);
		ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	// ── Constructor null checks ───────────────────────────────────────────────

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenNodeExecutionStateUpdaterIsNull()
	{
		Action act = () => _ = new NodeExecutor(null!, _serviceScopeFactory,
			_dataFlowConfigsStore, _dataFlowExecutionUserIdProvider, _dynamicParametersStore);

		act.Should().Throw<ArgumentNullException>().WithParameterName("nodeExecutionStateUpdater");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenServiceScopeFactoryIsNull()
	{
		Action act = () => _ = new NodeExecutor(_nodeExecutionStateUpdater,
			null!, _dataFlowConfigsStore, _dataFlowExecutionUserIdProvider, _dynamicParametersStore);

		act.Should().Throw<ArgumentNullException>().WithParameterName("serviceScopeFactory");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenNodeConfigsStoreIsNull()
	{
		Action act = () => _ = new NodeExecutor(_nodeExecutionStateUpdater,
			_serviceScopeFactory, null!, _dataFlowExecutionUserIdProvider, _dynamicParametersStore);

		act.Should().Throw<ArgumentNullException>().WithParameterName("nodeConfigsStore");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowExecutionUserIdProviderIsNull()
	{
		Action act = () => _ = new NodeExecutor(_nodeExecutionStateUpdater,
			_serviceScopeFactory, _dataFlowConfigsStore, null!, _dynamicParametersStore);

		act.Should().Throw<ArgumentNullException>().WithParameterName("dataFlowExecutionUserIdProvider");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDynamicParametersStoreIsNull()
	{
		Action act = () => _ = new NodeExecutor(_nodeExecutionStateUpdater,
			_serviceScopeFactory, _dataFlowConfigsStore, _dataFlowExecutionUserIdProvider, null!);

		act.Should().Throw<ArgumentNullException>().WithParameterName("dynamicParametersStore");
	}

	[Fact]
	public async Task ExecuteNodesAsync_ShouldSetExecutionStateToRunningForEachNode_WhenMultipleNodesProvided()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId1 = new NodeId(Guid.NewGuid());
		var nodeId2 = new NodeId(Guid.NewGuid());

		_dataFlowConfigsStore.GetNodeConfig(executionId, Arg.Any<NodeId>())
			.Returns(new NodeConfig { Content = "config", Version = new NodeVersion("1") });
		_dataFlowExecutionUserIdProvider.GetDataFlowExecutionUserId(executionId)
			.Returns(new UserId(Guid.NewGuid()));
		_dynamicParametersStore.GetDynamicParameters(executionId, Arg.Any<NodeId>()).Returns([]);

		// Act
		await _sut.ExecuteNodesAsync(executionId, new HashSet<NodeId> { nodeId1, nodeId2 }, CancellationToken.None);

		// Assert
		_nodeExecutionStateUpdater.Received()
			.ChangeNodeExecutionState(executionId, nodeId1, NodeExecutionState.Running);
		_nodeExecutionStateUpdater.Received()
			.ChangeNodeExecutionState(executionId, nodeId2, NodeExecutionState.Running);
	}

	[Fact]
	public async Task
		ExecuteNodesAsync_ShouldPublishExecuteNodeCommandWithDynamicParametersForEachNode_WhenMultipleNodesProvided()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId1 = new NodeId(Guid.NewGuid());
		var nodeId2 = new NodeId(Guid.NewGuid());

		var nodeConfig1 = new NodeConfig { Content = "node config1", Version = new NodeVersion("12") };
		_dataFlowConfigsStore.GetNodeConfig(executionId, nodeId1).Returns(nodeConfig1);
		var nodeConfig2 = new NodeConfig { Content = "node config2", Version = new NodeVersion("13") };
		_dataFlowConfigsStore.GetNodeConfig(executionId, nodeId2).Returns(nodeConfig2);

		var userId = new UserId(Guid.NewGuid());
		_dataFlowExecutionUserIdProvider.GetDataFlowExecutionUserId(executionId).Returns(userId);

		var paramsForNode1 = new List<DynamicParameter>();
		var paramsForNode2 = new List<DynamicParameter> { new() { Name = "age", Value = 18 } };
		_dynamicParametersStore.GetDynamicParameters(executionId, nodeId1).Returns(paramsForNode1);
		_dynamicParametersStore.GetDynamicParameters(executionId, nodeId2).Returns(paramsForNode2);

		// Act
		await _sut.ExecuteNodesAsync(executionId, new HashSet<NodeId> { nodeId1, nodeId2 }, CancellationToken.None);

		// Assert
		await _executionCommandPublisher.Received().PublishCommandAsync(
			Arg.Is<ExecuteNodeCommand>(command =>
				command.GraphExecutionId == executionId.Value &&
				command.UserId == userId.Value &&
				command.NodeId == nodeId1.Value &&
				command.PortConnections == null &&
				command.Config == nodeConfig1.Content &&
				command.DynamicParameters.Count == 0),
			CancellationToken.None);

		await _executionCommandPublisher.Received().PublishCommandAsync(
			Arg.Is<ExecuteNodeCommand>(command =>
				command.GraphExecutionId == executionId.Value &&
				command.UserId == userId.Value &&
				command.NodeId == nodeId2.Value &&
				command.PortConnections == null &&
				command.Config == nodeConfig2.Content &&
				command.DynamicParameters.Count == 1 &&
				command.DynamicParameters.First().Name == paramsForNode2[0].Name &&
				Equals(command.DynamicParameters.First().Value, paramsForNode2[0].Value)),
			CancellationToken.None);
	}

	[Fact]
	public async Task ExecuteNodesAsync_ShouldSetStateToRunningBeforePublishingCommands_WhenCalled()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());

		_dataFlowConfigsStore.GetNodeConfig(executionId, nodeId)
			.Returns(new NodeConfig { Content = "config", Version = new NodeVersion("1") });
		_dataFlowExecutionUserIdProvider.GetDataFlowExecutionUserId(executionId)
			.Returns(new UserId(Guid.NewGuid()));
		_dynamicParametersStore.GetDynamicParameters(executionId, nodeId).Returns([]);

		// Act
		await _sut.ExecuteNodesAsync(executionId, new HashSet<NodeId> { nodeId }, CancellationToken.None);

		// Assert
		Received.InOrder(() =>
		{
			_nodeExecutionStateUpdater.ChangeNodeExecutionState(executionId, nodeId, NodeExecutionState.Running);
			_executionCommandPublisher.PublishCommandAsync(Arg.Any<ExecuteNodeCommand>(), CancellationToken.None);
		});
	}
}
