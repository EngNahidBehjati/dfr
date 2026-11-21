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

public sealed class RemoveNodeIntegrationTests : IClassFixture<DataFlowBuilderServerFixture>,
	IClassFixture<AmServerFixture>,
	IDisposable
{
	private readonly WebApplicationFactory<Program> _factory;
	private readonly DataFlowChangeEventPublishHelper _publishHelper;
	private readonly DataFlowExecutionAssertionsHelper _assertionsHelper;
	private readonly RunDataFlowHelper _runDataFlowHelper;
	private readonly IDataFlowExecutionContextProvider _contextProvider;

	public RemoveNodeIntegrationTests(DataFlowBuilderServerFixture dataFlowBuilderServerFixture,
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
		_contextProvider = _factory.Services.GetRequiredService<IDataFlowExecutionContextProvider>();
	}

	[Fact]
	public async Task RemoveNode_ShouldRemoveNodeAAndInvalidateDownstream_WhenNodeAIsRemovedFromSampleTopology()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		var topologyDtoCreator = new TopologyDtoCreator();
		var changeEvent = new ChangeEvent(1, 2,
			[DataFlowChangeEventActionBuilder.RemovePluginAction(topologyDtoCreator.NodeANodeId)]);

		// Act
		await _runDataFlowHelper.Run(executionId, dataFlowVersion, topologyDtoCreator);
		await _publishHelper.PublishAndWaitAsync(executionId, changeEvent);

		// Assert
		_assertionsHelper.AssertNodeStates(executionId, new Dictionary<Guid, NodeExecutionState>
		{
			[topologyDtoCreator.RootNodeId] = NodeExecutionState.Succeeded,
			[topologyDtoCreator.NodeAChildNodeId] = NodeExecutionState.Idle,
			[topologyDtoCreator.NodeBNodeId] = NodeExecutionState.Succeeded,
			[topologyDtoCreator.JoinNodeId] = NodeExecutionState.Idle
		});
		DataFlowExecutionContext context = await _contextProvider
			.GetRunningDataFlowExecutionContextAsync(new DataFlowExecutionId(executionId), CancellationToken.None);
		context.Graph.NodeExists(new NodeId(topologyDtoCreator.NodeANodeId)).Should().BeFalse();
	}

	public void Dispose()
	{
		_factory.Dispose();
	}
}
