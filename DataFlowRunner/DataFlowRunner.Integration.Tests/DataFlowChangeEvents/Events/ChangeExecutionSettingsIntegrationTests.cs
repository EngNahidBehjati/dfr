using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Commands;
using DataFlowRunner.Integration.Tests.Common;
using DataFlowRunner.Integration.Tests.Common.Constants;
using DataFlowRunner.Integration.Tests.Common.Wiremock;
using DataFlowRunner.Integration.Tests.DataFlowChangeEvents.Helpers;
using DataFlowRunner.Integration.Tests.DataFlowExecution.Helpers;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Wolverine.Tracking;

namespace DataFlowRunner.Integration.Tests.DataFlowChangeEvents.Events;

public sealed class ChangeExecutionSettingsIntegrationTests : IClassFixture<DataFlowBuilderServerFixture>,
	IClassFixture<AmServerFixture>,
	IDisposable
{
	private readonly WebApplicationFactory<Program> _factory;
	private readonly DataFlowChangeEventPublishHelper _publishHelper;
	private readonly DataFlowExecutionAssertionsHelper _assertionsHelper;
	private readonly RunDataFlowHelper _runDataFlowHelper;
	private readonly CancelDataFlowHelper _cancelDataFlowHelper;
	private readonly IDataFlowExecutionContextProvider _contextProvider;

	public ChangeExecutionSettingsIntegrationTests(DataFlowBuilderServerFixture dataFlowBuilderServerFixture,
		AmServerFixture amServerFixture)
	{
		ArgumentNullException.ThrowIfNull(dataFlowBuilderServerFixture);
		ArgumentNullException.ThrowIfNull(amServerFixture);

		_factory = WebApplicationFactoryCreator.Create(
			dataFlowBuilderServerFixture.BaseAddress,
			amServerFixture.BaseAddress);

		_publishHelper = new DataFlowChangeEventPublishHelper(_factory);
		_assertionsHelper = new DataFlowExecutionAssertionsHelper(_factory);
		_runDataFlowHelper = new RunDataFlowHelper(_factory, dataFlowBuilderServerFixture);
		_cancelDataFlowHelper = new CancelDataFlowHelper(_factory);
		_contextProvider = _factory.Services.GetRequiredService<IDataFlowExecutionContextProvider>();
	}

	[Fact]
	public async Task ChangeExecutionSettings_ShouldChangeSucceededAndPendingToIdleAndRunningToCanceled_WhenDataFlowIsRunning()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var topologyDtoCreator = new TopologyDtoCreator();

		var wolverineStubOptions = new WolverineStubOptions
		{
			NodesWithoutExecutionReport = new HashSet<Guid> { topologyDtoCreator.NodeANodeId }
		};
		var changeEvent = new ChangeEvent(1, 2,
		[
			DataFlowChangeEventActionBuilder.ChangeExecutionSettingsAction("EXECUTORS_LIST", Guid.NewGuid())
		]);

		// Act
		await _runDataFlowHelper.Run(executionId, 1, topologyDtoCreator, wolverineStubOptions);
		ITrackedSession trackedSession = await _publishHelper.PublishAndWaitAsync(executionId, changeEvent);

		// Assert
		_assertionsHelper.AssertNodeStates(executionId, new Dictionary<Guid, NodeExecutionState>
		{
			[topologyDtoCreator.RootNodeId] = NodeExecutionState.Idle,
			[topologyDtoCreator.NodeANodeId] = NodeExecutionState.Canceled,
			[topologyDtoCreator.NodeAChildNodeId] = NodeExecutionState.Idle,
			[topologyDtoCreator.NodeBNodeId] = NodeExecutionState.Idle,
			[topologyDtoCreator.JoinNodeId] = NodeExecutionState.Idle
		});
	}

	[Fact]
	public async Task ChangeExecutionSettings_ShouldChangeCancelingNodeStateToCanceled_WhenNodeIsCanceling()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var topologyDtoCreator = new TopologyDtoCreator();

		var wolverineStubOptions = new WolverineStubOptions
		{
			NodesWithoutExecutionReport = new HashSet<Guid> { topologyDtoCreator.NodeANodeId },
			NodesWithoutCancelReport = new HashSet<Guid> { topologyDtoCreator.NodeANodeId }
		};
		var changeEvent = new ChangeEvent(1, 2,
		[
			DataFlowChangeEventActionBuilder.ChangeExecutionSettingsAction("EXECUTORS_LIST", Guid.NewGuid())
		]);

		// Act
		await _runDataFlowHelper.Run(executionId, 1, topologyDtoCreator, wolverineStubOptions);
		await _cancelDataFlowHelper.Cancel(executionId);

		_assertionsHelper.AssertNodeStates(executionId, new Dictionary<Guid, NodeExecutionState>
		{
			[topologyDtoCreator.RootNodeId] = NodeExecutionState.Succeeded,
			[topologyDtoCreator.NodeANodeId] = NodeExecutionState.Canceling,
			[topologyDtoCreator.NodeAChildNodeId] = NodeExecutionState.Idle,
			[topologyDtoCreator.NodeBNodeId] = NodeExecutionState.Succeeded,
			[topologyDtoCreator.JoinNodeId] = NodeExecutionState.Idle
		});

		await _publishHelper.PublishAndWaitAsync(executionId, changeEvent);

		// Assert
		_assertionsHelper.AssertNodeStates(executionId, new Dictionary<Guid, NodeExecutionState>
		{
			[topologyDtoCreator.RootNodeId] = NodeExecutionState.Idle,
			[topologyDtoCreator.NodeANodeId] = NodeExecutionState.Canceled,
			[topologyDtoCreator.NodeAChildNodeId] = NodeExecutionState.Idle,
			[topologyDtoCreator.NodeBNodeId] = NodeExecutionState.Idle,
			[topologyDtoCreator.JoinNodeId] = NodeExecutionState.Idle
		});
	}

	[Fact]
	public async Task ChangeExecutionSettings_ShouldPublishDisposeCommand_WhenExecutorSettingsChange()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var topologyDtoCreator = new TopologyDtoCreator();

		var wolverineStubOptions = new WolverineStubOptions
		{
			NodesWithoutExecutionReport = new HashSet<Guid> { topologyDtoCreator.NodeANodeId }
		};
		var changeEvent = new ChangeEvent(1, 2,
		[
			DataFlowChangeEventActionBuilder.ChangeExecutionSettingsAction("EXECUTORS_LIST", Guid.NewGuid())
		]);

		// Act
		await _runDataFlowHelper.Run(executionId, 1, topologyDtoCreator, wolverineStubOptions);
		ITrackedSession trackedSession = await _publishHelper.PublishAndWaitAsync(executionId, changeEvent);

		// Assert
		trackedSession.Sent.AllMessages()
			.OfType<DisposeDataFlowExecutionWolverineCommand>()
			.Where(command => command.GraphExecutionId == executionId)
			.Should().ContainSingle()
			.Which.UserId.Should().Be(AccessManagementTestConstants.TestRoleId);
	}

	[Fact]
	public async Task ChangeExecutionSettings_ShouldStoreExecutionSettingsAndVersion_WhenSettingsChangeSuccessfully()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var topologyDtoCreator = new TopologyDtoCreator();
		var executorId = Guid.NewGuid();

		var wolverineStubOptions = new WolverineStubOptions
		{
			NodesWithoutExecutionReport = new HashSet<Guid> { topologyDtoCreator.NodeANodeId }
		};
		var changeEvent = new ChangeEvent(1, 2,
		[
			DataFlowChangeEventActionBuilder.ChangeExecutionSettingsAction("EXECUTORS_LIST", executorId)
		]);

		// Act
		await _runDataFlowHelper.Run(executionId, 1, topologyDtoCreator, wolverineStubOptions);
		await _publishHelper.PublishAndWaitAsync(executionId, changeEvent);

		// Assert
		DataFlowExecutionContext context = await _contextProvider
			.GetRunningDataFlowExecutionContextAsync(new DataFlowExecutionId(executionId), CancellationToken.None);
		context.ExecutionSettings.ExecutorType.Should().Be("EXECUTORS_LIST");
		context.ExecutionSettings.ExecutorId.Should().Be(executorId);
		context.DataFlowVersion.Value.Should().Be(2);
	}

	public void Dispose()
	{
		_factory.Dispose();
	}
}
