using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Commands;
using DataFlowRunner.Integration.Tests.Common;
using DataFlowRunner.Integration.Tests.Common.Wiremock;
using DataFlowRunner.Integration.Tests.DataFlowChangeEvents.Helpers;
using DataFlowRunner.Integration.Tests.DataFlowExecution.Helpers;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Wolverine.Tracking;

namespace DataFlowRunner.Integration.Tests.DataFlowChangeEvents.Events;

public sealed class SetNodeConfigIntegrationTests : IClassFixture<DataFlowBuilderServerFixture>,
	IClassFixture<AmServerFixture>,
	IDisposable
{
	private readonly WebApplicationFactory<Program> _factory;
	private readonly DataFlowChangeEventPublishHelper _publishHelper;
	private readonly DataFlowExecutionAssertionsHelper _assertionsHelper;
	private readonly RunDataFlowHelper _runDataFlowHelper;

	public SetNodeConfigIntegrationTests(DataFlowBuilderServerFixture dataFlowBuilderServerFixture,
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
	}

	[Fact]
	public async Task SetNodeConfig_ShouldCancelRunningDownstreamAndInvalidateOtherDownstreamNodes_WhenRunningNodeConfigIsSet()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		var topologyDtoCreator = new TopologyDtoCreator();
		var wolverineStubOptions = new WolverineStubOptions
		{
			NodesWithoutExecutionReport = new HashSet<Guid> { topologyDtoCreator.NodeANodeId }
		};

		var changeEvent = new ChangeEvent(1, 2,
			[DataFlowChangeEventActionBuilder.SetPluginConfigAction(topologyDtoCreator.NodeANodeId)]);

		// Act
		await _runDataFlowHelper.Run(executionId, dataFlowVersion, topologyDtoCreator, wolverineStubOptions);
		ITrackedSession trackedSession = await _publishHelper.PublishAndWaitAsync(executionId, changeEvent);

		// Assert
		_assertionsHelper.AssertNodeStates(executionId, new Dictionary<Guid, NodeExecutionState>
		{
			[topologyDtoCreator.RootNodeId] = NodeExecutionState.Succeeded,
			[topologyDtoCreator.NodeANodeId] = NodeExecutionState.Canceled,
			[topologyDtoCreator.NodeAChildNodeId] = NodeExecutionState.Idle,
			[topologyDtoCreator.NodeBNodeId] = NodeExecutionState.Succeeded,
			[topologyDtoCreator.JoinNodeId] = NodeExecutionState.Idle
		});
		var publishedCancelNodeCommandNodeIds = trackedSession
			.Sent
			.AllMessages()
			.OfType<CancelNodeWolverineCommand>()
			.Where(command => command.GraphExecutionId == executionId)
			.Select(command => command.NodeId)
			.ToList();
		publishedCancelNodeCommandNodeIds.Should().BeEquivalentTo([topologyDtoCreator.NodeANodeId]);
	}

	public void Dispose()
	{
		_factory.Dispose();
	}
}
