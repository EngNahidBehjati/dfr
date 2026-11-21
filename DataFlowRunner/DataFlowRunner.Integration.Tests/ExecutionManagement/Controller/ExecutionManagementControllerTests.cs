using System.Net;
using System.Text;
using System.Text.Json;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.ExecutionManagement.Models;
using DataFlowRunner.Integration.Tests.Common;
using DataFlowRunner.Integration.Tests.Common.Constants;
using DataFlowRunner.Integration.Tests.Common.Wiremock;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Wolverine;
using Wolverine.Tracking;

namespace DataFlowRunner.Integration.Tests.ExecutionManagement.Controller;

public class ExecutionManagementControllerTests : IClassFixture<AmServerFixture>
{
	private readonly HttpClient _httpClient;
	private readonly IHost _host;
	private readonly IDataFlowExecutionFacade _mockDataFlowExecutionFacade;
	private const string Url = "api/execution-management/cancel";

	public ExecutionManagementControllerTests(AmServerFixture amServerFixture)
	{
		_mockDataFlowExecutionFacade = Substitute.For<IDataFlowExecutionFacade>()!;

		WebApplicationFactory<Program> factory =
			WebApplicationFactoryCreator.Create(amAddress: amServerFixture.BaseAddress)
				.WithWebHostBuilder(builder =>
				{
					builder.ConfigureTestServices(services =>
					{
						services.RemoveAll<IDataFlowExecutionFacade>();
						services.AddSingleton(_mockDataFlowExecutionFacade);
					});
				});

		_host = factory.Services.GetRequiredService<IHost>();

		_httpClient = factory.CreateClient();
		_httpClient.DefaultRequestHeaders.Add("Cookie", AccessManagementTestConstants.AuthCookie);
	}

	[Fact]
	public async Task CancelExecution_ShouldPublishDataFlowExecutionFacade_WhenExecutionSourceIsWebDataFlowPage()
	{
		// Arrange
		var command = new CancelCommandDto
		{
			ExecutionSource = "WEB_DATA_FLOW_PAGE",
			DataFlowId = Guid.NewGuid(),
			DataFlowExecutionId = Guid.NewGuid()
		};
		var requestBody = JsonSerializer.Serialize(command);

		// Act
		using HttpResponseMessage response = await SendRequest(requestBody);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		await _mockDataFlowExecutionFacade.Received()
			.CancelDataFlow(command.DataFlowExecutionId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CancelExecution_ShouldPublishCancelExecutionCommand_WhenExecutionSourceIsNotWebDataFlowPage()
	{
		// Arrange
		var command = new CancelCommandDto
		{
			ExecutionSource = "API",
			DataFlowId = Guid.NewGuid(),
			DataFlowExecutionId = Guid.NewGuid()
		};
		var requestBody = JsonSerializer.Serialize(command);

		Task<HttpResponseMessage> responseTask = null!;
		Func<IMessageContext, Task> callApi = _ =>
		{
			responseTask = SendRequest(requestBody);
			return responseTask;
		};
		var expectedCancelCommand = new CancelExecutionCommand
		{
			ExecutionSource = "API",
			DataFlowId = command.DataFlowId,
			DataFlowExecutionId = command.DataFlowExecutionId
		};

		// Act
		IEnumerable<CancelExecutionCommand> commands = await CallApiAndGetPublishedCommands(callApi);

		// Assert
		using HttpResponseMessage response = await responseTask;
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		commands.Should().BeEquivalentTo([expectedCancelCommand]);
	}

	[Fact]
	public async Task CancelExecution_ShouldReturnBadRequest_WhenExecutionSourceIsInvalid()
	{
		// Arrange
		var command = new CancelCommandDto
		{
			ExecutionSource = "INVALID_SOURCE",
			DataFlowId = Guid.NewGuid(),
			DataFlowExecutionId = Guid.NewGuid()
		};
		var requestBody = JsonSerializer.Serialize(command);

		Task<HttpResponseMessage> responseTask = null!;
		Func<IMessageContext, Task> callApi = _ =>
		{
			responseTask = SendRequest(requestBody);
			return responseTask;
		};

		// Act
		IEnumerable<CancelExecutionCommand> commands = await CallApiAndGetPublishedCommands(callApi);

		// Assert
		using HttpResponseMessage response = await responseTask;
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		commands.Should().BeEmpty();
		await _mockDataFlowExecutionFacade.DidNotReceive()
			.CancelDataFlow(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
	}

	private async Task<IEnumerable<CancelExecutionCommand>> CallApiAndGetPublishedCommands(
		Func<IMessageContext, Task> callApi)
	{
		ITrackedSession session = await _host.Services
			.TrackActivity()
			.Timeout(TimeSpan.FromSeconds(20))
			.ExecuteAndWaitAsync(callApi);

		return session.Sent
			.AllMessages()
			.OfType<CancelExecutionCommand>();
	}

	private async Task<HttpResponseMessage> SendRequest(string requestBody)
	{
		using var request = new HttpRequestMessage(HttpMethod.Post, Url);
		request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
		return await _httpClient.SendAsync(request);
	}
}
