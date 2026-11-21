using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace DataFlowRunner.Integration.Tests.Common.Wiremock;

public sealed class DataFlowBuilderServerFixture : IAsyncLifetime
{
	private const string JsonContentType = "application/json";
	private WireMockServer _server = null!;
	public string BaseAddress => _server.Url ?? throw new InvalidOperationException("WireMock server is not initialized.");

	public Task InitializeAsync()
	{
		_server = WireMockServer.Start();
		return Task.CompletedTask;
	}

	public void StubJsonResponse(string path, HttpMethod httpMethod, object? body = null, int statusCode = StatusCodes.Status200OK)
	{
		IResponseBuilder responseBuilder = Response.Create()
			.WithStatusCode(statusCode);

		if (body is not null)
		{
			var serializedBody = JsonSerializer.Serialize(body);
			responseBuilder
				.WithHeader(HeaderNames.ContentType, JsonContentType)
				.WithBody(serializedBody);
		}

		_server
			.Given(Request.Create()
				.WithPath(path)
				.UsingMethod(httpMethod.Method))
			.RespondWith(responseBuilder);
	}

	public Task DisposeAsync()
	{
		_server.Stop();
		_server.Dispose();
		return Task.CompletedTask;
	}
}
