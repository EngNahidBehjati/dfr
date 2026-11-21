using System.Net.Http.Json;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Integration.Tests.Common;
using DataFlowRunner.Integration.Tests.Common.Constants;
using DataFlowRunner.Integration.Tests.Common.Wiremock;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.DataTransferObjects;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DataFlowRunner.Integration.Tests.NodeExecutionStates.Controllers;

public sealed class DataFlowExecutionStateControllerTests : IClassFixture<AmServerFixture>
{
	private readonly HttpClient _httpClient;
	private readonly INodeExecutionStateStore _stateStore;
	private readonly INodeExecutionStateUpdater _stateUpdater;
	private readonly IDataFlowExecutionStatusStore _dataFlowExecutionStatusStore;

	public DataFlowExecutionStateControllerTests(AmServerFixture amServerFixture)
	{
		WebApplicationFactory<Program> factory =
			WebApplicationFactoryCreator.Create(amAddress: amServerFixture.BaseAddress);
		_stateStore = factory.Services.GetRequiredService<INodeExecutionStateStore>();
		_stateUpdater = factory.Services.GetRequiredService<INodeExecutionStateUpdater>();
		_dataFlowExecutionStatusStore = factory.Services.GetRequiredService<IDataFlowExecutionStatusStore>();
		_httpClient = factory.CreateClient();
		_httpClient.DefaultRequestHeaders.Add("Cookie", AccessManagementTestConstants.AuthCookie);
	}

	[Fact]
	public async Task GetNodeStates_ShouldReturnStates_WhenStatesWereSetForExecutionId()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());

		var nodeId1 = new NodeId(Guid.NewGuid());
		var nodeId2 = new NodeId(Guid.NewGuid());
		var nodeId3 = new NodeId(Guid.NewGuid());
		_stateStore.SetNodeExecutionState(dataFlowExecutionId, nodeId1, NodeExecutionState.Running);
		_stateStore.SetNodeExecutionState(dataFlowExecutionId, nodeId2, NodeExecutionState.Pending);
		_stateStore.SetNodeExecutionState(dataFlowExecutionId, nodeId3, NodeExecutionState.Idle);

		var url = BuildNodeStatesUrl(dataFlowExecutionId.Value);

		var expectedStates = new DataFlowNodeExecutionStatesDto([
			new NodeExecutionStateByIdDto
			{
				NodeId = nodeId1.Value,
				NodeExecutionStateDto = NodeExecutionStateDto.Running
			},
			new NodeExecutionStateByIdDto
			{
				NodeId = nodeId2.Value,
				NodeExecutionStateDto = NodeExecutionStateDto.Pending
			},
			new NodeExecutionStateByIdDto
			{
				NodeId = nodeId3.Value,
				NodeExecutionStateDto = NodeExecutionStateDto.Idle
			}
		]);

		// Act
		using var request = new HttpRequestMessage(HttpMethod.Get, url);
		using HttpResponseMessage response = await _httpClient.SendAsync(request);

		// Assert
		response.Should().BeSuccessful();

		DataFlowNodeExecutionStatesDto? states =
			await response.Content.ReadFromJsonAsync<DataFlowNodeExecutionStatesDto>(ApiSerializationDefaults.Options);

		states.Should().BeEquivalentTo(expectedStates);
	}

	[Fact]
	public async Task GetNodeStates_ShouldReturnStatesAndExecutionTime_WhenNodeExecuted()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		_stateStore.SetNodeExecutionState(dataFlowExecutionId, nodeId, NodeExecutionState.Idle);
		_stateUpdater.ChangeNodeExecutionState(dataFlowExecutionId, nodeId, NodeExecutionState.Pending);
		_stateUpdater.ChangeNodeExecutionState(dataFlowExecutionId, nodeId, NodeExecutionState.Running);
		await Task.Delay(1000);
		_stateUpdater.ChangeNodeExecutionState(dataFlowExecutionId, nodeId, NodeExecutionState.Succeeded);

		var url = BuildNodeStatesUrl(dataFlowExecutionId.Value);

		// Act
		using var request = new HttpRequestMessage(HttpMethod.Get, url);
		using HttpResponseMessage response = await _httpClient.SendAsync(request);

		// Assert
		response.Should().BeSuccessful();

		DataFlowNodeExecutionStatesDto? states =
			await response.Content.ReadFromJsonAsync<DataFlowNodeExecutionStatesDto>(ApiSerializationDefaults.Options);

		NodeExecutionStateByIdDto node = states!.NodeExecutionStates.Should().ContainSingle().Which;
		node.NodeId.Should().Be(nodeId.Value);
		node.NodeExecutionStateDto.Should().Be(NodeExecutionStateDto.Succeeded);
		node.DurationMilliseconds.Should().BeGreaterThanOrEqualTo(990);
	}

	[Fact]
	public async Task GetExecutionState_ShouldReturnNodeStatesAndExecutionStatus_WhenStatesWereSetForExecutionId()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());

		var nodeId1 = new NodeId(Guid.NewGuid());
		var nodeId2 = new NodeId(Guid.NewGuid());
		_stateStore.SetNodeExecutionState(dataFlowExecutionId, nodeId1, NodeExecutionState.Running);
		_stateStore.SetNodeExecutionState(dataFlowExecutionId, nodeId2, NodeExecutionState.Pending);

		var url = BuildExecutionStateUrl(dataFlowExecutionId.Value);

		var nodeStates = new DataFlowNodeExecutionStatesDto([
			new NodeExecutionStateByIdDto
			{
				NodeId = nodeId1.Value,
				NodeExecutionStateDto = NodeExecutionStateDto.Running
			},
			new NodeExecutionStateByIdDto
			{
				NodeId = nodeId2.Value,
				NodeExecutionStateDto = NodeExecutionStateDto.Pending
			}
		]);

		var expected = new DataFlowExecutionStatusDto
		{
			NodeExecutionStates = nodeStates.NodeExecutionStates,
			ExecutionStatus = new ExecutionStatusDto
			{
				Failed = true,
				Message = "failed Message"
			}
		};

		var dataFlowExecutionStatus = new DataFlowExecutionStatus("failed Message");
		_dataFlowExecutionStatusStore.Set(dataFlowExecutionId, dataFlowExecutionStatus);

		// Act
		using var request = new HttpRequestMessage(HttpMethod.Get, url);
		using HttpResponseMessage response = await _httpClient.SendAsync(request);

		// Assert
		response.Should().BeSuccessful();

		DataFlowExecutionStatusDto? states =
			await response.Content.ReadFromJsonAsync<DataFlowExecutionStatusDto>(ApiSerializationDefaults.Options);

		states.Should().BeEquivalentTo(expected);
	}

	private static string BuildNodeStatesUrl(Guid executionId) =>
		$"api/data-flow-executions/{executionId}/node-states";

	private static string BuildExecutionStateUrl(Guid executionId) =>
		$"api/data-flow-executions/{executionId}/execution-state";
}
