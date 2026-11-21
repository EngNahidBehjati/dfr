using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Dto;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Commands;
using DataFlowRunner.DataFlowSnapshot.Models;
using DataFlowRunner.Integration.Tests.Common;
using DataFlowRunner.Integration.Tests.Common.Constants;
using DataFlowRunner.Integration.Tests.Common.Wiremock;
using DataFlowRunner.Integration.Tests.DataFlowExecution.Helpers;
using DataFlowRunner.Integration.Tests.DataFlowExecution.Models;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine.Tracking;

namespace DataFlowRunner.Integration.Tests.DataFlowExecution.Controllers;

public sealed class RunDataFlowEndpointTests : IClassFixture<DataFlowBuilderServerFixture>,
	IClassFixture<AmServerFixture>,
	IDisposable
{
	private const string NodeCanceledMessage = "The plugin execution was canceled by the previous plugin.";
	private readonly WebApplicationFactory<Program> _factory;
	private readonly HttpClient _httpClient;
	private readonly DataFlowBuilderApisStubHelper _dataFlowBuilderApisStubHelper;
	private readonly DataFlowExecutionRequestHelper _requestHelper;
	private readonly DataFlowExecutionAssertionsHelper _assertionsHelper;
	private readonly WolverineStubConfiguration _wolverineStubConfiguration;

	public RunDataFlowEndpointTests(DataFlowBuilderServerFixture dataFlowBuilderServerFixture,
		AmServerFixture amServerFixture)
	{
		ArgumentNullException.ThrowIfNull(dataFlowBuilderServerFixture);
		ArgumentNullException.ThrowIfNull(amServerFixture);

		_factory = WebApplicationFactoryCreator.Create(
			dataFlowBuilderServerFixture.BaseAddress,
			amServerFixture.BaseAddress);
		_httpClient = _factory.CreateClient();
		_httpClient.DefaultRequestHeaders.Add("Cookie", AccessManagementTestConstants.AuthCookie);

		_dataFlowBuilderApisStubHelper = new DataFlowBuilderApisStubHelper(dataFlowBuilderServerFixture);
		_requestHelper = new DataFlowExecutionRequestHelper(_factory);
		_assertionsHelper = new DataFlowExecutionAssertionsHelper(_factory);
		_wolverineStubConfiguration = new WolverineStubConfiguration(_factory);
	}

	[Fact]
	public async Task RunDataFlow_ShouldPublishExecutionCommandsAndMarkNodesSucceeded_WhenAllNodesSucceed()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		TopologyDto topology = new TopologyDtoCreator().Create();
		_dataFlowBuilderApisStubHelper.StubTopologyResponse(executionId, topology, dataFlowVersion);
		_dataFlowBuilderApisStubHelper.StubNodeConfigsResponse(executionId, topology);
		_dataFlowBuilderApisStubHelper.StubDynamicParametersResponse(executionId);
		_wolverineStubConfiguration.ConfigureWolverineStubs(new WolverineStubOptions());

		var expectedNodeIds = topology.Nodes.Select(node => node.NodeId).ToList();
		var runDataFlowUrl = BuildUrlHelper.BuildRunDataFlowUrl(executionId, dataFlowVersion);
		var expectedNodeStates = topology.Nodes.ToDictionary(node => node.NodeId, _ => NodeExecutionState.Succeeded);

		// Act
		DataFlowExecutionResult result = await _requestHelper.ExecuteRequestAsync(runDataFlowUrl);

		// Assert
		_assertionsHelper.AssertStatusCode(result, HttpStatusCode.OK);
		_assertionsHelper.AssertNodeStates(executionId, expectedNodeStates);
		var publishedExecuteNodeCommandNodeIds = result
			.GetPublishedCommands<ExecuteNodeWolverineCommand>(executionId)
			.Select(command => command.NodeId)
			.ToList();
		publishedExecuteNodeCommandNodeIds.Should().BeEquivalentTo(expectedNodeIds);
		InitializeDataFlowExecutionWolverineCommand initializeCommand = result
			.GetPublishedCommands<InitializeDataFlowExecutionWolverineCommand>(executionId)
			.Should().ContainSingle().Which;
		initializeCommand.ProcessorInfo.ProcessorType.Should().Be(topology.ProcessorInfo.ProcessorType);
		initializeCommand.ProcessorInfo.ProcessorId.Should().Be(topology.ProcessorInfo.ProcessorId);
	}

	[Fact]
	public async Task RunDataFlow_ShouldExecuteNodesSuccessfully_WhenAllDynamicParametersAreProvided()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		TopologyDto topology = new TopologyDtoCreator().Create();
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

		var runDataFlowUrl = BuildUrlHelper.BuildRunDataFlowUrl(executionId, dataFlowVersion);
		var expectedNodeStates = topology.Nodes.ToDictionary(node => node.NodeId, _ => NodeExecutionState.Succeeded);
		var body = new RunDataFlowDto
		{
			DynamicParameters = new List<DynamicParameterExecutionDto>
			{
				new()
				{
					Name = "age",
					Value = 1
				}
			}
		};

		// Act
		DataFlowExecutionResult result = await _requestHelper.ExecuteRequestAsync(runDataFlowUrl, body);

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
	public async Task RunDataFlow_ShouldReturnBadRequest_WhenSomeDynamicParameterNotProvided()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		TopologyDto topology = new TopologyDtoCreator().Create();
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

		var runDataFlowUrl = BuildUrlHelper.BuildRunDataFlowUrl(executionId, dataFlowVersion);
		var expectedNodeStates = topology.Nodes.ToDictionary(node => node.NodeId, _ => NodeExecutionState.Idle);

		// Act
		DataFlowExecutionResult result = await _requestHelper.ExecuteRequestAsync(runDataFlowUrl, new RunDataFlowDto());

		// Assert
		_assertionsHelper.AssertStatusCode(result, HttpStatusCode.BadRequest);
		_assertionsHelper.AssertNodeStates(executionId, expectedNodeStates);
	}

	[Fact]
	public async Task RunDataFlow_ShouldReturnBadRequest_WhenRequiredDynamicParameterValueIsNull()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		TopologyDto topology = new TopologyDtoCreator().Create();
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

		var runDataFlowUrl = BuildUrlHelper.BuildRunDataFlowUrl(executionId, dataFlowVersion);
		var expectedNodeStates = topology.Nodes.ToDictionary(node => node.NodeId, _ => NodeExecutionState.Idle);
		var body = new RunDataFlowDto
		{
			DynamicParameters = new List<DynamicParameterExecutionDto>
			{
				new()
				{
					Name = "age",
					Value = null
				}
			}
		};

		// Act
		DataFlowExecutionResult result = await _requestHelper.ExecuteRequestAsync(runDataFlowUrl, body);

		// Assert
		_assertionsHelper.AssertStatusCode(result, HttpStatusCode.BadRequest);
		_assertionsHelper.AssertNodeStates(executionId, expectedNodeStates);
	}

	[Fact]
	public async Task RunDataFlow_ShouldExecuteNodesSuccessfully_WhenOptionalDynamicParameterValueIsNull()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		TopologyDto topology = new TopologyDtoCreator().Create();
		_dataFlowBuilderApisStubHelper.StubTopologyResponse(executionId, topology, dataFlowVersion);
		_dataFlowBuilderApisStubHelper.StubNodeConfigsResponse(executionId, topology);
		var dynamicParameterNames = new List<ResponseDataFlowDynamicParameterDto>
		{
			CreateResponseDynamicParameter("age", false)
		};
		_dataFlowBuilderApisStubHelper.StubDynamicParametersResponse(executionId,
			topology.Nodes.ToDictionary(
				node => node.NodeId,
				_ => (IReadOnlyCollection<ResponseDataFlowDynamicParameterDto>)dynamicParameterNames));
		_wolverineStubConfiguration.ConfigureWolverineStubs(new WolverineStubOptions());

		var runDataFlowUrl = BuildUrlHelper.BuildRunDataFlowUrl(executionId, dataFlowVersion);
		var expectedNodeStates = topology.Nodes.ToDictionary(node => node.NodeId, _ => NodeExecutionState.Succeeded);
		var body = new RunDataFlowDto
		{
			DynamicParameters = new List<DynamicParameterExecutionDto>
			{
				new()
				{
					Name = "age",
					Value = null
				}
			}
		};

		// Act
		DataFlowExecutionResult result = await _requestHelper.ExecuteRequestAsync(runDataFlowUrl, body);

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
			parameters.First().Value.Should().BeNull();
		});
	}

	[Fact]
	public async Task RunDataFlow_ShouldExecuteEachNodeJustOnce_WhenMultipleRequestsSentParallel()
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
		var expectedExecutedNodeIds = topology.Nodes.Select(node => node.NodeId).ToList();

		IHost host = _factory.Services.GetRequiredService<IHost>();
		var responses = new ConcurrentBag<HttpResponseMessage>();

		// Act
		ITrackedSession session = await host.Services
			.TrackActivity()
			.Timeout(TimeSpan.FromSeconds(20))
			.ExecuteAndWaitAsync(_ =>
			{
				IEnumerable<Task> tasks = Enumerable.Range(0, 50)
					.Select(async _ =>
					{
						HttpResponseMessage response = await _httpClient.PostAsync(runDataFlowUrl, null);
						responses.Add(response);
					});
				return Task.WhenAll(tasks);
			});

		// Assert
		responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
		var executeNodeCommands = session.Sent
			.AllMessages()
			.OfType<ExecuteNodeWolverineCommand>()
			.Where(x => x.GraphExecutionId == executionId)
			.Select(x => x.NodeId)
			.ToList();
		executeNodeCommands.Should().BeEquivalentTo(expectedExecutedNodeIds);
	}

	[Fact]
	public async Task RunDataFlow_ShouldSaveErrorMessageAndSetNodeStateToFailed_WhenNodeExecutionFails()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		TopologyDto topology = new TopologyDtoCreator().Create();
		_dataFlowBuilderApisStubHelper.StubTopologyResponse(executionId, topology, dataFlowVersion);
		_dataFlowBuilderApisStubHelper.StubNodeConfigsResponse(executionId, topology);
		_dataFlowBuilderApisStubHelper.StubDynamicParametersResponse(executionId);

		List<Guid> succeededNodeIds = [topology.Nodes.ElementAt(0).NodeId, topology.Nodes.ElementAt(3).NodeId];
		List<Guid> cancelledNodeIds = [topology.Nodes.ElementAt(2).NodeId, topology.Nodes.ElementAt(4).NodeId];
		Guid failedNodeId = topology.Nodes.ElementAt(1).NodeId;
		var expectedErrorMessage = new NodeErrorMessage
		{
			Message = "Node execution failed: test error message",
			StackTrace = "test stacktrace"
		};
		var wolverineStubOptions = new WolverineStubOptions
		{
			NodeFailureMessages = new Dictionary<Guid, NodeErrorMessage> { [failedNodeId] = expectedErrorMessage }
		};
		_wolverineStubConfiguration.ConfigureWolverineStubs(wolverineStubOptions);

		var runDataFlowUrl = BuildUrlHelper.BuildRunDataFlowUrl(executionId, dataFlowVersion);
		var expectedNodeStates = topology.Nodes.ToDictionary(node => node.NodeId, node =>
		{
			if (succeededNodeIds.Contains(node.NodeId))
			{
				return NodeExecutionState.Succeeded;
			}

			if (cancelledNodeIds.Contains(node.NodeId))
			{
				return NodeExecutionState.Canceled;
			}

			return NodeExecutionState.Failed;
		});
		var expectedErrorMessages = cancelledNodeIds.ToDictionary(
			nodeId => nodeId,
			_ => new NodeErrorMessage { Message = NodeCanceledMessage, StackTrace = null });
		expectedErrorMessages[failedNodeId] = expectedErrorMessage;

		// Act
		DataFlowExecutionResult result = await _requestHelper.ExecuteRequestAsync(runDataFlowUrl);

		// Assert
		_assertionsHelper.AssertStatusCode(result, HttpStatusCode.OK);
		_assertionsHelper.AssertNodeStates(executionId, expectedNodeStates);
		_assertionsHelper.AssertErrorMessage(executionId, expectedErrorMessages);
	}

	[Fact]
	public async Task RunDataFlow_ShouldRunUnExecutedSubgraph_WhenCalledForTheSecondTime()
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

		var runDataFlowUrl = BuildUrlHelper.BuildRunDataFlowUrl(executionId, dataFlowVersion);

		var expectedExecuteNodeCommandNodeIds = new List<Guid>
		{
			topologyDtoCreator.NodeANodeId,
			topologyDtoCreator.NodeAChildNodeId,
			topologyDtoCreator.JoinNodeId
		};
		var expectedNodeStates = topology.Nodes
			.ToDictionary(node => node.NodeId, _ => NodeExecutionState.Succeeded);

		// Act
		DataFlowExecutionResult firstExecutionResult = await _requestHelper.ExecuteRequestAsync(runDataFlowUrl);
		_wolverineStubConfiguration.ConfigureWolverineStubs(new WolverineStubOptions());
		DataFlowExecutionResult secondExecutionResult = await _requestHelper.ExecuteRequestAsync(runDataFlowUrl);

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

	[Fact]
	public async Task RunDataFlow_ShouldReturnDataFlowBuilderError_WhenGettingConfigsFails()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		var topologyDtoCreator = new TopologyDtoCreator();
		TopologyDto topology = topologyDtoCreator.Create();
		_dataFlowBuilderApisStubHelper.StubTopologyResponse(executionId, topology, dataFlowVersion);
		const string stacktrace = "get configs stack trace";
		const string message = "get configs failed";
		_dataFlowBuilderApisStubHelper.StubNodeConfigsFailResponse(executionId, message, stacktrace);
		_dataFlowBuilderApisStubHelper.StubDynamicParametersResponse(executionId);

		var runDataFlowUrl = BuildUrlHelper.BuildRunDataFlowUrl(executionId, dataFlowVersion);

		// Act
		DataFlowExecutionResult executionResult = await _requestHelper.ExecuteRequestAsync(runDataFlowUrl);

		// Assert
		_assertionsHelper.AssertStatusCode(executionResult, HttpStatusCode.InternalServerError);
		executionResult.GetPublishedCommands<ExecuteNodeWolverineCommand>(executionId)
			.Should().BeEmpty();

		ProblemDetails? problem = await executionResult.HttpResponseContainer.Response.Content
			.ReadFromJsonAsync<ProblemDetails>();
		problem.Should().NotBeNull();
		problem.Type.Should().Be("Internal Server Error");
		problem.Title.Should().Contain(message);
		problem.Detail.Should().Contain(stacktrace);
	}

	[Fact]
	public async Task RunDataFlow_ShouldReturnDataFlowBuilderError_WhenGettingTopologyFails()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		const string stacktrace = "get topology stack trace";
		const string message = "get topology failed";
		_dataFlowBuilderApisStubHelper.StubTopologyFailResponse(executionId, message, stacktrace);
		_dataFlowBuilderApisStubHelper.StubDynamicParametersResponse(executionId);

		var runDataFlowUrl = BuildUrlHelper.BuildRunDataFlowUrl(executionId, dataFlowVersion);

		// Act
		DataFlowExecutionResult executionResult = await _requestHelper.ExecuteRequestAsync(runDataFlowUrl);

		// Assert
		_assertionsHelper.AssertStatusCode(executionResult, HttpStatusCode.InternalServerError);
		executionResult.GetPublishedCommands<ExecuteNodeWolverineCommand>(executionId)
			.Should().BeEmpty();

		ProblemDetails? problem = await executionResult.HttpResponseContainer.Response.Content
			.ReadFromJsonAsync<ProblemDetails>();
		problem.Should().NotBeNull();
		problem.Type.Should().Be("Internal Server Error");
		problem.Title.Should().Contain(message);
		problem.Detail.Should().Contain(stacktrace);
	}

	public void Dispose()
	{
		_httpClient.Dispose();
		_factory.Dispose();
	}

	private static ResponseDataFlowDynamicParameterDto CreateResponseDynamicParameter(
		string name, bool required = true)
	{
		return new ResponseDataFlowDynamicParameterDto(
			Guid.NewGuid().ToString(),
			name,
			required,
			"INTEGER",
			false,
			1);
	}
}
