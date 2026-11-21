using System.Net;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Commands;
using DataFlowRunner.DataFlowSnapshot.Models;
using DataFlowRunner.Integration.Tests.Common;
using DataFlowRunner.Integration.Tests.Common.Wiremock;
using DataFlowRunner.Integration.Tests.DataFlowExecution.Helpers;
using DataFlowRunner.Integration.Tests.DataFlowExecution.Models;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DataFlowRunner.Integration.Tests.DataFlowExecution.Controllers;

public sealed class InvalidateDataFlowEndpointTests : IClassFixture<DataFlowBuilderServerFixture>,
	IClassFixture<AmServerFixture>,
	IDisposable
{
	private readonly WebApplicationFactory<Program> _factory;
	private readonly DataFlowBuilderApisStubHelper _dataFlowBuilderApisStubHelper;
	private readonly DataFlowExecutionRequestHelper _requestHelper;
	private readonly DataFlowExecutionAssertionsHelper _assertionsHelper;
	private readonly WolverineStubConfiguration _wolverineStubConfiguration;

	public InvalidateDataFlowEndpointTests(DataFlowBuilderServerFixture dataFlowBuilderServerFixture,
		AmServerFixture amServerFixture)
	{
		ArgumentNullException.ThrowIfNull(dataFlowBuilderServerFixture);
		ArgumentNullException.ThrowIfNull(amServerFixture);

		_factory = WebApplicationFactoryCreator.Create(
			dataFlowBuilderServerFixture.BaseAddress,
			amServerFixture.BaseAddress);

		_dataFlowBuilderApisStubHelper = new DataFlowBuilderApisStubHelper(dataFlowBuilderServerFixture);
		_requestHelper = new DataFlowExecutionRequestHelper(_factory);
		_assertionsHelper = new DataFlowExecutionAssertionsHelper(_factory);
		_wolverineStubConfiguration = new WolverineStubConfiguration(_factory);
	}

	[Fact]
	public async Task InvalidateDataFlow_ShouldSetNodeStatesToIdle_WhenAllNodesExecutedSuccessfullyBeforeInvalidation()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		TopologyDto topology = new TopologyDtoCreator().Create();
		_dataFlowBuilderApisStubHelper.StubTopologyResponse(executionId, topology, dataFlowVersion);
		_dataFlowBuilderApisStubHelper.StubNodeConfigsResponse(executionId, topology);
		_dataFlowBuilderApisStubHelper.StubDynamicParametersResponse(executionId);
		_wolverineStubConfiguration.ConfigureWolverineStubs(new WolverineStubOptions());

		var runDataFlowUrl = BuildUrlHelper.BuildRunDataFlowUrl(executionId, dataFlowVersion);
		var invalidateDataFlowUrl = BuildUrlHelper.BuildInvalidateDataFlowUrl(executionId);
		var expectedNodeStates = topology.Nodes
			.ToDictionary(node => node.NodeId, _ => NodeExecutionState.Idle);

		// Act
		await _requestHelper.ExecuteRequestAsync(runDataFlowUrl);
		DataFlowExecutionResult result = await _requestHelper.ExecuteRequestAsync(invalidateDataFlowUrl);

		// Assert
		_assertionsHelper.AssertStatusCode(result, HttpStatusCode.OK);
		_assertionsHelper.AssertNodeStates(executionId, expectedNodeStates);
		IEnumerable<CancelNodeWolverineCommand> publishedCommandsNodeIds =
			result.GetPublishedCommands<CancelNodeWolverineCommand>(executionId);
		publishedCommandsNodeIds.Should().BeEmpty();
	}

	[Fact]
	public async Task InvalidateDataFlow_ShouldSetStateOfSucceededNodeToIdleAndRunningNodeToCanceled_WhenExactlyOneNodeIsRunning()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		TopologyDto topology = new TopologyDtoCreator().Create();
		_dataFlowBuilderApisStubHelper.StubTopologyResponse(executionId, topology, dataFlowVersion);
		_dataFlowBuilderApisStubHelper.StubNodeConfigsResponse(executionId, topology);
		_dataFlowBuilderApisStubHelper.StubDynamicParametersResponse(executionId);
		Guid runningNodeId = topology.Nodes.ElementAt(1).NodeId;
		var wolverineStubOptions = new WolverineStubOptions
		{
			NodesWithoutExecutionReport = new HashSet<Guid> { runningNodeId }
		};
		_wolverineStubConfiguration.ConfigureWolverineStubs(wolverineStubOptions);

		var runDataFlowUrl = BuildUrlHelper.BuildRunDataFlowUrl(executionId, dataFlowVersion);
		var invalidateDataFlowUrl = BuildUrlHelper.BuildInvalidateDataFlowUrl(executionId);
		var expectedNodeStates = topology.Nodes.ToDictionary(
			node => node.NodeId,
			node => node.NodeId == runningNodeId ? NodeExecutionState.Canceled : NodeExecutionState.Idle);

		// Act
		await _requestHelper.ExecuteRequestAsync(runDataFlowUrl);
		DataFlowExecutionResult result = await _requestHelper.ExecuteRequestAsync(invalidateDataFlowUrl);

		// Assert
		_assertionsHelper.AssertStatusCode(result, HttpStatusCode.OK);
		_assertionsHelper.AssertNodeStates(executionId, expectedNodeStates);
		var publishedCancelNodeCommandsNodeIds =
			result.GetPublishedCommands<CancelNodeWolverineCommand>(executionId)
				.Select(command => command.NodeId).ToList();
		publishedCancelNodeCommandsNodeIds.Should().ContainSingle().Which.Should().Be(runningNodeId);
	}

	public void Dispose()
	{
		_factory.Dispose();
	}
}
