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

public sealed class InvalidateDataFlowNodeEndpointTests : IClassFixture<DataFlowBuilderServerFixture>,
	IClassFixture<AmServerFixture>,
	IDisposable
{
	private readonly WebApplicationFactory<Program> _factory;
	private readonly DataFlowBuilderApisStubHelper _dataFlowBuilderApisStubHelper;
	private readonly DataFlowExecutionRequestHelper _requestHelper;
	private readonly DataFlowExecutionAssertionsHelper _assertionsHelper;
	private readonly WolverineStubConfiguration _wolverineStubConfiguration;

	public InvalidateDataFlowNodeEndpointTests(DataFlowBuilderServerFixture dataFlowBuilderServerFixture,
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
	public async Task InvalidateDataFlowNode_ShouldPublishCancelNodeCommandAndSetPendingDownstreamToIdle_WhenTargetNodeIsRunning()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		var topologyDtoCreator = new TopologyDtoCreator();
		TopologyDto topology = topologyDtoCreator.Create();
		_dataFlowBuilderApisStubHelper.StubTopologyResponse(executionId, topology, dataFlowVersion);
		_dataFlowBuilderApisStubHelper.StubNodeConfigsResponse(executionId, topology);
		_dataFlowBuilderApisStubHelper.StubDynamicParametersResponse(executionId);
		var wolverineStubOptions = new WolverineStubOptions
		{
			NodesWithoutExecutionReport = new HashSet<Guid> { topologyDtoCreator.NodeANodeId }
		};
		_wolverineStubConfiguration.ConfigureWolverineStubs(wolverineStubOptions);

		var runDataFlowUrl = BuildUrlHelper.BuildRunDataFlowUrl(executionId, dataFlowVersion);
		var invalidateDataFlowNodeUrl = BuildUrlHelper.BuildInvalidateDataFlowNodeUrl(
			executionId,
			topologyDtoCreator.NodeANodeId);
		var expectedNodeStates = new Dictionary<Guid, NodeExecutionState>
		{
			{ topologyDtoCreator.RootNodeId, NodeExecutionState.Succeeded },
			{ topologyDtoCreator.NodeANodeId, NodeExecutionState.Canceled },
			{ topologyDtoCreator.NodeAChildNodeId, NodeExecutionState.Idle },
			{ topologyDtoCreator.NodeBNodeId, NodeExecutionState.Succeeded },
			{ topologyDtoCreator.JoinNodeId, NodeExecutionState.Idle }
		};

		// Act
		await _requestHelper.ExecuteRequestAsync(runDataFlowUrl);
		DataFlowExecutionResult result = await _requestHelper.ExecuteRequestAsync(invalidateDataFlowNodeUrl);

		// Assert
		_assertionsHelper.AssertStatusCode(result, HttpStatusCode.OK);
		_assertionsHelper.AssertNodeStates(executionId, expectedNodeStates);
		var publishedCancelNodeCommandNodeIds = result
			.GetPublishedCommands<CancelNodeWolverineCommand>(executionId)
			.Select(command => command.NodeId)
			.ToList();
		publishedCancelNodeCommandNodeIds.Should().ContainSingle().Which.Should().Be(topologyDtoCreator.NodeANodeId);
	}

	[Fact]
	public async Task InvalidateDataFlowNode_ShouldSetSourceAndDownstreamSucceededNodesToIdle_WhenAllNodesExecutedSuccessfullyBeforeInvalidation()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		var topologyDtoCreator = new TopologyDtoCreator();
		TopologyDto topology = topologyDtoCreator.Create();
		_dataFlowBuilderApisStubHelper.StubTopologyResponse(executionId, topology, dataFlowVersion);
		_dataFlowBuilderApisStubHelper.StubNodeConfigsResponse(executionId, topology);
		_dataFlowBuilderApisStubHelper.StubDynamicParametersResponse(executionId);
		_wolverineStubConfiguration.ConfigureWolverineStubs(new WolverineStubOptions());

		var runDataFlowUrl = BuildUrlHelper.BuildRunDataFlowUrl(executionId, dataFlowVersion);
		var invalidateDataFlowNodeUrl = BuildUrlHelper.BuildInvalidateDataFlowNodeUrl(
			executionId,
			topologyDtoCreator.NodeANodeId);
		var expectedNodeStates = new Dictionary<Guid, NodeExecutionState>
		{
			{ topologyDtoCreator.RootNodeId, NodeExecutionState.Succeeded },
			{ topologyDtoCreator.NodeANodeId, NodeExecutionState.Idle },
			{ topologyDtoCreator.NodeAChildNodeId, NodeExecutionState.Idle },
			{ topologyDtoCreator.NodeBNodeId, NodeExecutionState.Succeeded },
			{ topologyDtoCreator.JoinNodeId, NodeExecutionState.Idle }
		};

		// Act
		await _requestHelper.ExecuteRequestAsync(runDataFlowUrl);
		DataFlowExecutionResult result = await _requestHelper.ExecuteRequestAsync(invalidateDataFlowNodeUrl);

		// Assert
		_assertionsHelper.AssertStatusCode(result, HttpStatusCode.OK);
		_assertionsHelper.AssertNodeStates(executionId, expectedNodeStates);
		IEnumerable<CancelNodeWolverineCommand> publishedCancelNodeCommands =
			result.GetPublishedCommands<CancelNodeWolverineCommand>(executionId);
		publishedCancelNodeCommands.Should().BeEmpty();
	}

	[Fact]
	public async Task InvalidateDataFlowNode_ShouldSetSucceededNodesToIdleAndCancelRunningDownstream_WhenSourceNodeSucceededAndSomeDownStreamNodeIsRunning()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		var topologyDtoCreator = new TopologyDtoCreator();
		TopologyDto topology = topologyDtoCreator.Create();
		_dataFlowBuilderApisStubHelper.StubTopologyResponse(executionId, topology, dataFlowVersion);
		_dataFlowBuilderApisStubHelper.StubNodeConfigsResponse(executionId, topology);
		_dataFlowBuilderApisStubHelper.StubDynamicParametersResponse(executionId);
		var wolverineStubOptions = new WolverineStubOptions
		{
			NodesWithoutExecutionReport = new HashSet<Guid> { topologyDtoCreator.NodeAChildNodeId }
		};
		_wolverineStubConfiguration.ConfigureWolverineStubs(wolverineStubOptions);

		var runDataFlowUrl = BuildUrlHelper.BuildRunDataFlowUrl(executionId, dataFlowVersion);
		var invalidateDataFlowNodeUrl = BuildUrlHelper.BuildInvalidateDataFlowNodeUrl(
			executionId,
			topologyDtoCreator.RootNodeId);
		var expectedNodeStates = new Dictionary<Guid, NodeExecutionState>
		{
			{ topologyDtoCreator.RootNodeId, NodeExecutionState.Idle },
			{ topologyDtoCreator.NodeANodeId, NodeExecutionState.Idle },
			{ topologyDtoCreator.NodeAChildNodeId, NodeExecutionState.Canceled },
			{ topologyDtoCreator.NodeBNodeId, NodeExecutionState.Idle },
			{ topologyDtoCreator.JoinNodeId, NodeExecutionState.Idle }
		};

		// Act
		await _requestHelper.ExecuteRequestAsync(runDataFlowUrl);
		DataFlowExecutionResult result = await _requestHelper.ExecuteRequestAsync(invalidateDataFlowNodeUrl);

		// Assert
		_assertionsHelper.AssertStatusCode(result, HttpStatusCode.OK);
		_assertionsHelper.AssertNodeStates(executionId, expectedNodeStates);
		var publishedCancelNodeCommandNodeIds = result
			.GetPublishedCommands<CancelNodeWolverineCommand>(executionId)
			.Select(command => command.NodeId)
			.ToList();
		publishedCancelNodeCommandNodeIds.Should().ContainSingle().Which.Should().Be(topologyDtoCreator.NodeAChildNodeId);
	}

	public void Dispose()
	{
		_factory.Dispose();
	}
}
