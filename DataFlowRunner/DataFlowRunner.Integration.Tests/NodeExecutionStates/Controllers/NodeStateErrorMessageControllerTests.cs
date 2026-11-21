using System.Net;
using System.Net.Http.Json;
using DataFlowRunner.Integration.Tests.Common;
using DataFlowRunner.Integration.Tests.Common.Constants;
using DataFlowRunner.Integration.Tests.Common.Wiremock;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.DataTransferObjects;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DataFlowRunner.Integration.Tests.NodeExecutionStates.Controllers;

public sealed class NodeStateErrorMessageControllerTests :
	IClassFixture<AmServerFixture>
{
	private readonly WebApplicationFactory<Program> _factory;
	private readonly HttpClient _httpClient;

	public NodeStateErrorMessageControllerTests(AmServerFixture amServerFixture)
	{
		_factory = WebApplicationFactoryCreator.Create(amAddress: amServerFixture.BaseAddress);

		_httpClient = _factory.CreateClient();
		_httpClient.DefaultRequestHeaders.Add("Cookie", AccessManagementTestConstants.AuthCookie);
	}

	[Fact]
	public async Task GetNodeErrorMessage_ShouldReturnOkWithErrorMessage_WhenErrorMessageExists()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		const string expectedErrorMessage = "Node execution failed: timeout occurred";
		const string expectedStackTrace = "expected stacktrace";
		var errorMessage = new NodeErrorMessage { Message = expectedErrorMessage, StackTrace = expectedStackTrace };

		INodeErrorMessageStore errorMessageStore = _factory.Services.GetRequiredService<INodeErrorMessageStore>();
		errorMessageStore.SetNodeErrorMessage(
			executionId,
			nodeId,
			errorMessage);

		var requestUrl = BuildGetNodeErrorMessageUrl(executionId, nodeId);
		var expectedDto = new NodeErrorMessageDto
		{
			ErrorMessage = expectedErrorMessage,
			StackTrace = expectedStackTrace
		};

		// Act
		HttpResponseMessage response = await SendGetRequestAsync(requestUrl);

		// Assert
		response.Should().BeSuccessful();
		var errorMessageDto = await response.Content.ReadFromJsonAsync<NodeErrorMessageDto>(ApiSerializationDefaults.Options);
		errorMessageDto.Should().BeEquivalentTo(expectedDto);
	}

	[Fact]
	public async Task GetNodeErrorMessage_ShouldReturnNotFound_WhenDataFlowExecutionDoesNotExist()
	{
		// Arrange
		var nonExistentExecutionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var requestUrl = BuildGetNodeErrorMessageUrl(nonExistentExecutionId, nodeId);

		// Act
		HttpResponseMessage response = await SendGetRequestAsync(requestUrl);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
		ProblemDetails? problemDetails =
			await response.Content.ReadFromJsonAsync<ProblemDetails>(ApiSerializationDefaults.Options);
		problemDetails?.Status.Should().Be(StatusCodes.Status404NotFound);
	}

	[Fact]
	public async Task GetNodeErrorMessage_ShouldReturnNotFound_WhenNodeErrorMessageDoesNotExist()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var existingNodeId = Guid.NewGuid();
		var missingNodeId = Guid.NewGuid();
		const string existingErrorMessage = "Existing error message";
		const string existingStackTrace = "Existing stacktrace";
		var errorMessage = new NodeErrorMessage { Message = existingErrorMessage, StackTrace = existingStackTrace };

		INodeErrorMessageStore errorMessageStore = _factory.Services.GetRequiredService<INodeErrorMessageStore>();
		errorMessageStore.SetNodeErrorMessage(
			executionId,
			existingNodeId,
			errorMessage);

		var requestUrl = BuildGetNodeErrorMessageUrl(executionId, missingNodeId);

		// Act
		HttpResponseMessage response = await SendGetRequestAsync(requestUrl);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
		ProblemDetails? problemDetails =
			await response.Content.ReadFromJsonAsync<ProblemDetails>(ApiSerializationDefaults.Options);
		problemDetails?.Status.Should().Be(StatusCodes.Status404NotFound);
	}

	private async Task<HttpResponseMessage> SendGetRequestAsync(string requestUrl)
	{
		using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
		return await _httpClient.SendAsync(request).ConfigureAwait(false);
	}

	private static string BuildGetNodeErrorMessageUrl(Guid executionId, Guid nodeId) =>
		$"api/data-flow-executions/{executionId}/node-states/{nodeId}/error-message";
}
