using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowSnapshot.Models;
using DataFlowRunner.Integration.Tests.Common;
using DataFlowRunner.Integration.Tests.Common.Wiremock;
using DataFlowRunner.Integration.Tests.DataFlowChangeEvents.Helpers;
using DataFlowRunner.Integration.Tests.DataFlowExecution.Helpers;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DataFlowRunner.Integration.Tests.DataFlowChangeEvents.Events;

public sealed class DataFlowMultiChangeEventsIntegrationTests : IClassFixture<DataFlowBuilderServerFixture>,
	IClassFixture<AmServerFixture>,
	IDisposable
{
	private readonly WebApplicationFactory<Program> _factory;
	private readonly DataFlowExecutionAssertionsHelper _assertionsHelper;
	private readonly RunDataFlowHelper _runDataFlowHelper;
	private readonly DataFlowChangeEventPublishHelper _publishHelper;
	private readonly IDataFlowExecutionContextProvider _contextProvider;

	public DataFlowMultiChangeEventsIntegrationTests(
		DataFlowBuilderServerFixture dataFlowBuilderServerFixture,
		AmServerFixture amServerFixture)
	{
		ArgumentNullException.ThrowIfNull(dataFlowBuilderServerFixture);
		ArgumentNullException.ThrowIfNull(amServerFixture);

		_factory = WebApplicationFactoryCreator
			.Create(dataFlowBuilderServerFixture.BaseAddress, amServerFixture.BaseAddress);

		_assertionsHelper = new DataFlowExecutionAssertionsHelper(_factory);
		_runDataFlowHelper = new RunDataFlowHelper(_factory, dataFlowBuilderServerFixture);
		_publishHelper = new DataFlowChangeEventPublishHelper(_factory);

		_contextProvider = _factory.Services.GetRequiredService<IDataFlowExecutionContextProvider>();
	}

	[Fact]
	public async Task RunDataFlow_ShouldSucceedAllNodes_WhenSampleTopologyIsBuiltFromChangeEvents()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long initialPipelineVersion = 1;

		SampleTopologyChangeEventActions topologyActions = new();
		TopologyDto initialTopology = CreateEmptyTopology();

		var expectedNodeStates = topologyActions.NodeIds
			.ToDictionary(nodeId => nodeId, _ => NodeExecutionState.Succeeded);

		// Act
		await _runDataFlowHelper.Run(executionId, initialPipelineVersion, initialTopology, new WolverineStubOptions());

		await _publishHelper.PublishAndWaitAsync(
			executionId,
			new ChangeEvent(1, 2, topologyActions.CreateAddPluginActions()));
		await _publishHelper.PublishAndWaitAsync(
			executionId,
			new ChangeEvent(1, 3, topologyActions.CreateAddLinkActions()));

		DataFlowExecutionContext runningContext = await _contextProvider
			.GetRunningDataFlowExecutionContextAsync(new DataFlowExecutionId(executionId), CancellationToken.None);
		TopologyDto builtTopology = CreateTopologyDto(runningContext);

		await _runDataFlowHelper.Run(executionId, 3, builtTopology, new WolverineStubOptions());

		// Assert
		_assertionsHelper.AssertNodeStates(executionId, expectedNodeStates);
		runningContext.DataFlowVersion.Value.Should().Be(3);
	}

	public void Dispose()
	{
		_factory.Dispose();
	}

	private static TopologyDto CreateEmptyTopology()
	{
		return new TopologyDto([], [], new ProcessorInfoDto("PROCESSOR_LIST", Guid.NewGuid()),
			new ExecutionSettingsDto("PIPELINE_EXECUTOR", null));
	}

	private static TopologyDto CreateTopologyDto(DataFlowExecutionContext context)
	{
		ArgumentNullException.ThrowIfNull(context);

		var nodeIndex = 0;
		var nodes = context.Graph.GetAllNodes()
			.Select(node => new NodeDto(node.Id.Value, "Default", [], Version: 99 + nodeIndex++))
			.ToList();
		var edges = context.Graph.GetAllEdges()
			.Select(edge => new EdgeDto(
				edge.Id.Value,
				Guid.NewGuid(),
				Guid.NewGuid(),
				edge.FromNodeId.Value,
				edge.ToNodeId.Value))
			.ToList();

		return new TopologyDto(nodes, edges, new ProcessorInfoDto("PROCESSOR_LIST", Guid.NewGuid()),
			new ExecutionSettingsDto("PIPELINE_EXECUTOR", null));
	}
}
