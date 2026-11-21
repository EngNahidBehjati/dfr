using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Integration.Tests.Common;
using DataFlowRunner.Integration.Tests.Common.Wiremock;
using DataFlowRunner.Integration.Tests.DataFlowChangeEvents.Helpers;
using DataFlowRunner.Integration.Tests.DataFlowExecution.Helpers;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DataFlowRunner.Integration.Tests.DataFlowChangeEvents.Events;

public sealed class DataFlowChangeEventsGuardClausesIntegrationTests : IClassFixture<DataFlowBuilderServerFixture>,
	IClassFixture<AmServerFixture>,
	IDisposable
{
	private readonly WebApplicationFactory<Program> _factory;
	private readonly DataFlowChangeEventPublishHelper _publishHelper;
	private readonly DataFlowExecutionAssertionsHelper _assertionsHelper;
	private readonly IDataFlowExecutionContextProvider _contextProvider;
	private readonly RunDataFlowHelper _runDataFlowHelper;

	public DataFlowChangeEventsGuardClausesIntegrationTests(DataFlowBuilderServerFixture dataFlowBuilderServerFixture,
		AmServerFixture amServerFixture)
	{
		_factory = WebApplicationFactoryCreator.Create(
			dataFlowBuilderServerFixture.BaseAddress,
			amServerFixture.BaseAddress);

		_publishHelper = new DataFlowChangeEventPublishHelper(_factory);
		_assertionsHelper = new DataFlowExecutionAssertionsHelper(_factory);
		_runDataFlowHelper = new RunDataFlowHelper(_factory, dataFlowBuilderServerFixture);
		_contextProvider = _factory.Services.GetRequiredService<IDataFlowExecutionContextProvider>();
	}

	[Fact]
	public async Task MultipleActions_ShouldApplyValidActions_WhenEventContainsUnknownActionType()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var newNodeId = Guid.NewGuid();

		var changeEvent = new ChangeEvent(
			1,
			2,
			[
				DataFlowChangeEventActionBuilder.UnknownTypeAction(),
				DataFlowChangeEventActionBuilder.AddPluginAction(newNodeId)
			]);

		var topologyDtoCreator = new TopologyDtoCreator();

		// Act
		await _runDataFlowHelper.Run(executionId, 1, topologyDtoCreator);
		await _publishHelper.PublishAndWaitAsync(executionId, changeEvent);

		// Assert
		_assertionsHelper.AssertNodeStates(executionId, new Dictionary<Guid, NodeExecutionState>
		{
			[newNodeId] = NodeExecutionState.Idle,
			[topologyDtoCreator.RootNodeId] = NodeExecutionState.Succeeded,
			[topologyDtoCreator.NodeANodeId] = NodeExecutionState.Succeeded,
			[topologyDtoCreator.NodeAChildNodeId] = NodeExecutionState.Succeeded,
			[topologyDtoCreator.NodeBNodeId] = NodeExecutionState.Succeeded,
			[topologyDtoCreator.JoinNodeId] = NodeExecutionState.Succeeded
		});
		DataFlowExecutionContext context = await _contextProvider
			.GetRunningDataFlowExecutionContextAsync(new DataFlowExecutionId(executionId), CancellationToken.None);
		context.Graph.NodeExists(new NodeId(newNodeId)).Should().BeTrue();
	}

	[Fact]
	public async Task MultipleActions_ShouldApplyValidActions_WhenEventContainsActionWithoutType()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var newNodeId = Guid.NewGuid();

		var changeEvent = new ChangeEvent(
			1,
			2,
			[
				DataFlowChangeEventActionBuilder.ActionMissingTypeProperty(newNodeId),
				DataFlowChangeEventActionBuilder.AddPluginAction(newNodeId)
			]);

		var topologyDtoCreator = new TopologyDtoCreator();

		// Act
		await _runDataFlowHelper.Run(executionId, 1, topologyDtoCreator);
		await _publishHelper.PublishAndWaitAsync(executionId, changeEvent);

		// Assert
		_assertionsHelper.AssertNodeStates(executionId, new Dictionary<Guid, NodeExecutionState>
		{
			[newNodeId] = NodeExecutionState.Idle,
			[topologyDtoCreator.RootNodeId] = NodeExecutionState.Succeeded,
			[topologyDtoCreator.NodeANodeId] = NodeExecutionState.Succeeded,
			[topologyDtoCreator.NodeAChildNodeId] = NodeExecutionState.Succeeded,
			[topologyDtoCreator.NodeBNodeId] = NodeExecutionState.Succeeded,
			[topologyDtoCreator.JoinNodeId] = NodeExecutionState.Succeeded
		});
		DataFlowExecutionContext context = await _contextProvider
			.GetRunningDataFlowExecutionContextAsync(new DataFlowExecutionId(executionId), CancellationToken.None);
		context.Graph.NodeExists(new NodeId(newNodeId)).Should().BeTrue();
	}

	public void Dispose()
	{
		_factory.Dispose();
	}
}
