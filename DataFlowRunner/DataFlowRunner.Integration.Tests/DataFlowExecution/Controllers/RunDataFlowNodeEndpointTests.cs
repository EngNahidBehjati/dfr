using System.Net;
using System.Text.Json;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Dto;
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

public sealed class RunDataFlowNodeEndpointTests : IClassFixture<DataFlowBuilderServerFixture>,
	IClassFixture<AmServerFixture>,
	IDisposable
{
	private readonly WebApplicationFactory<Program> _factory;
	private readonly DataFlowBuilderApisStubHelper _dataFlowBuilderApisStubHelper;
	private readonly DataFlowExecutionRequestHelper _requestHelper;
	private readonly DataFlowExecutionAssertionsHelper _assertionsHelper;
	private readonly WolverineStubConfiguration _wolverineStubConfiguration;

	public RunDataFlowNodeEndpointTests(DataFlowBuilderServerFixture dataFlowBuilderServerFixture,
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
	public async Task RunDataFlowNode_ShouldPublishExecutionCommandsForUpstreamSubgraph_WhenAllUpstreamNodesExecutedSuccessfully()
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

		var runDataFlowNodeUrl = BuildUrlHelper.BuildRunDataFlowNodeUrl(
			executionId,
			topologyDtoCreator.NodeAChildNodeId,
			dataFlowVersion);
		var expectedExecuteNodeCommandNodeIds = new List<Guid>
		{
			topologyDtoCreator.RootNodeId,
			topologyDtoCreator.NodeANodeId,
			topologyDtoCreator.NodeAChildNodeId
		};
		var expectedNodeStates = new Dictionary<Guid, NodeExecutionState>
		{
			{ topologyDtoCreator.RootNodeId, NodeExecutionState.Succeeded },
			{ topologyDtoCreator.NodeANodeId, NodeExecutionState.Succeeded },
			{ topologyDtoCreator.NodeAChildNodeId, NodeExecutionState.Succeeded },
			{ topologyDtoCreator.NodeBNodeId, NodeExecutionState.Idle },
			{ topologyDtoCreator.JoinNodeId, NodeExecutionState.Idle }
		};

		// Act
		DataFlowExecutionResult result = await _requestHelper.ExecuteRequestAsync(runDataFlowNodeUrl);

		// Assert
		_assertionsHelper.AssertStatusCode(result, HttpStatusCode.OK);
		_assertionsHelper.AssertNodeStates(executionId, expectedNodeStates);
		var publishedExecuteNodeCommandNodeIds = result
			.GetPublishedCommands<ExecuteNodeWolverineCommand>(executionId)
			.Select(command => command.NodeId)
			.ToList();
		publishedExecuteNodeCommandNodeIds.Should().BeEquivalentTo(expectedExecuteNodeCommandNodeIds);
	}

	[Fact]
	public async Task RunDataFlowNode_ShouldExecuteUpstreamSubgraphSuccessfully_WhenAllDynamicParametersAreProvided()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		var topologyDtoCreator = new TopologyDtoCreator();
		TopologyDto topology = topologyDtoCreator.Create();
		_dataFlowBuilderApisStubHelper.StubTopologyResponse(executionId, topology, dataFlowVersion);
		_dataFlowBuilderApisStubHelper.StubNodeConfigsResponse(executionId, topology);
		var dynamicParameterNames = new List<ResponseDataFlowDynamicParameterDto>
		{
			CreateResponseDynamicParameter("age")
		};
		_dataFlowBuilderApisStubHelper.StubDynamicParametersResponse(executionId,
			topology.Nodes.ToDictionary(
				node => node.NodeId,
				_ => (IReadOnlyCollection<ResponseDataFlowDynamicParameterDto>)dynamicParameterNames));
		_wolverineStubConfiguration.ConfigureWolverineStubs(new WolverineStubOptions());

		var runDataFlowNodeUrl = BuildUrlHelper.BuildRunDataFlowNodeUrl(
			executionId,
			topologyDtoCreator.NodeAChildNodeId,
			dataFlowVersion);
		var expectedNodeStates = new Dictionary<Guid, NodeExecutionState>
		{
			{ topologyDtoCreator.RootNodeId, NodeExecutionState.Succeeded },
			{ topologyDtoCreator.NodeANodeId, NodeExecutionState.Succeeded },
			{ topologyDtoCreator.NodeAChildNodeId, NodeExecutionState.Succeeded },
			{ topologyDtoCreator.NodeBNodeId, NodeExecutionState.Idle },
			{ topologyDtoCreator.JoinNodeId, NodeExecutionState.Idle }
		};
		var body = new RunDataFlowNodeDto
		{
			DynamicParameters =
			[
				new DynamicParameterExecutionDto
				{
					Name = "age",
					Value = 1
				}
			]
		};

		// Act
		DataFlowExecutionResult result = await _requestHelper.ExecuteRequestAsync(runDataFlowNodeUrl, body);

		// Assert
		_assertionsHelper.AssertStatusCode(result, HttpStatusCode.OK);
		_assertionsHelper.AssertNodeStates(executionId, expectedNodeStates);
		var publishedExecuteNodeCommandDynamicParameters = result
			.GetPublishedCommands<ExecuteNodeWolverineCommand>(executionId)
			.Select(command => command.DynamicParameters)
			.ToList();
		publishedExecuteNodeCommandDynamicParameters.Should().AllSatisfy(parameters =>
		{
			parameters.Count.Should().Be(1);
			parameters.First().Name.Should().Be("age");
			parameters.First().Value.Should().NotBeNull();
			var value = ((JsonElement)parameters.First().Value!).GetInt32();
			value.Should().Be(1);
		});
	}

	[Fact]
	public async Task RunDataFlowNode_ShouldReturnBadRequest_WhenSomeDynamicParameterNotProvided()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		TopologyDto topology = new TopologyDtoCreator().Create();
		_dataFlowBuilderApisStubHelper.StubTopologyResponse(executionId, topology, dataFlowVersion);
		_dataFlowBuilderApisStubHelper.StubNodeConfigsResponse(executionId, topology);
		var dynamicParameterNames = new List<ResponseDataFlowDynamicParameterDto>
		{
			CreateResponseDynamicParameter("age"),
			CreateResponseDynamicParameter("name")
		};
		_dataFlowBuilderApisStubHelper.StubDynamicParametersResponse(executionId,
			topology.Nodes.ToDictionary(
				node => node.NodeId,
				_ => (IReadOnlyCollection<ResponseDataFlowDynamicParameterDto>)dynamicParameterNames));
		_wolverineStubConfiguration.ConfigureWolverineStubs(new WolverineStubOptions());

		Guid targetNodeId = topology.Nodes.ElementAt(2).NodeId;
		var runDataFlowNodeUrl = BuildUrlHelper.BuildRunDataFlowNodeUrl(executionId, targetNodeId, dataFlowVersion);
		var expectedNodeStates = topology.Nodes
			.ToDictionary(node => node.NodeId, _ => NodeExecutionState.Idle);
		var body = new RunDataFlowNodeDto
		{
			DynamicParameters =
			[
				new DynamicParameterExecutionDto
				{
					Name = "age",
					Value = 1
				}
			]
		};

		// Act
		DataFlowExecutionResult result = await _requestHelper.ExecuteRequestAsync(runDataFlowNodeUrl, body);

		// Assert
		_assertionsHelper.AssertStatusCode(result, HttpStatusCode.BadRequest);
		_assertionsHelper.AssertNodeStates(executionId, expectedNodeStates);
	}

	[Fact]
	public async Task RunDataFlowNode_ShouldRunUnExecutedUpstreamSubgraph_WhenCalledForTheSecondTime()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		var topologyDtoCreator = new TopologyDtoCreator();
		TopologyDto topology = topologyDtoCreator.Create();
		_dataFlowBuilderApisStubHelper.StubTopologyResponse(executionId, topology, dataFlowVersion);
		_dataFlowBuilderApisStubHelper.StubNodeConfigsResponse(executionId, topology);
		_dataFlowBuilderApisStubHelper.StubDynamicParametersResponse(executionId);

		Guid failedNodeId = topologyDtoCreator.NodeANodeId;
		var expectedErrorMessage = new NodeErrorMessage { Message = "Node execution failed", StackTrace = null };
		var wolverineStubOptions = new WolverineStubOptions
		{
			NodeFailureMessages = new Dictionary<Guid, NodeErrorMessage> { [failedNodeId] = expectedErrorMessage }
		};
		_wolverineStubConfiguration.ConfigureWolverineStubs(wolverineStubOptions);

		var runDataFlowNodeUrl = BuildUrlHelper.BuildRunDataFlowNodeUrl(
			executionId,
			topologyDtoCreator.JoinNodeId,
			dataFlowVersion);
		var expectedExecuteNodeCommandNodeIds = new List<Guid>
		{
			topologyDtoCreator.NodeANodeId,
			topologyDtoCreator.NodeAChildNodeId,
			topologyDtoCreator.JoinNodeId
		};
		var expectedNodeStates = topology.Nodes
			.ToDictionary(node => node.NodeId, _ => NodeExecutionState.Succeeded);

		// Act
		DataFlowExecutionResult firstExecutionResult = await _requestHelper.ExecuteRequestAsync(runDataFlowNodeUrl);
		_wolverineStubConfiguration.ConfigureWolverineStubs(new WolverineStubOptions());
		DataFlowExecutionResult secondExecutionResult = await _requestHelper.ExecuteRequestAsync(runDataFlowNodeUrl);

		// Assert
		_assertionsHelper.AssertStatusCode(firstExecutionResult, HttpStatusCode.OK);
		_assertionsHelper.AssertStatusCode(secondExecutionResult, HttpStatusCode.OK);
		var publishedExecuteNodeCommandNodeIds = secondExecutionResult
			.GetPublishedCommands<ExecuteNodeWolverineCommand>(executionId)
			.Select(command => command.NodeId)
			.ToList();

		publishedExecuteNodeCommandNodeIds.Should().BeEquivalentTo(expectedExecuteNodeCommandNodeIds);
		_assertionsHelper.AssertNodeStates(executionId, expectedNodeStates);
	}

	public void Dispose()
	{
		_factory.Dispose();
	}

	private static ResponseDataFlowDynamicParameterDto CreateResponseDynamicParameter(string name)
	{
		return new ResponseDataFlowDynamicParameterDto(
			Guid.NewGuid().ToString(),
			name,
			true,
			"INTEGER",
			false,
			1);
	}
}
