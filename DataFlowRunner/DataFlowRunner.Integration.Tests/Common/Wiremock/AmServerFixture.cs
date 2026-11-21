using System.Net;
using System.Text.Json;
using DataFlowRunner.Integration.Tests.Common.Constants;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace DataFlowRunner.Integration.Tests.Common.Wiremock;

public sealed class AmServerFixture : IAsyncLifetime
{
	private WireMockServer? _server;

	public string BaseAddress =>
		_server?.Url ?? throw new InvalidOperationException("WireMock server is not initialized.");

	public Task InitializeAsync()
	{
		_server = WireMockServer.Start(new WireMockServerSettings()
		{
			AllowPartialMapping = false
		});
		MockIsAuthenticate();
		MockGetLoginUser();
		MockHasServiceAccesses();
		return Task.CompletedTask;
	}

	private void MockIsAuthenticate()
	{
		_server!
			.Given(Request.Create()
				.WithPath("/api/UserManagementApi/IsAuthenticate")
				.WithHeader("X-Auth-Token", AccessManagementTestConstants.AuthTokenValue)
				.UsingPost())
			.RespondWith(Response.Create()
				.WithStatusCode(HttpStatusCode.OK));
	}

	private void MockHasServiceAccesses()
	{
		_server!
			.Given(Request.Create()
				.WithPath("/api/SheetManagementApi/HasServiceAccesses*")
				.UsingPost())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "application/json")
				.WithBody("true"));
	}

	private void MockGetLoginUser()
	{
		var userInfoModel = new
		{
			AuthenticationUserId = AccessManagementTestConstants.TestAuthenticationUserId,
			DisplayName = AccessManagementTestConstants.TestUserName,
			Id = new Guid(AccessManagementTestConstants.TestRoleId),
			LongId = AccessManagementTestConstants.TestUserId,
			UserName = AccessManagementTestConstants.TestUserName
		};

		var serializedResponse = JsonSerializer.Serialize(userInfoModel);

		_server!
			.Given(Request.Create()
				.WithPath("/api/UserManagementApi/GetLoginUser")
				.WithHeader("X-Auth-Token", AccessManagementTestConstants.AuthTokenValue)
				.UsingPost())
			.RespondWith(Response.Create()
				.WithBody(serializedResponse)
				.WithStatusCode(HttpStatusCode.OK));

		_server!
			.Given(Request.Create()
				.WithPath("/api/UserManagementApi/GetLoginUser")
				.WithParam("InputArgumentsSerializationName", "FastJsonNet")
				.WithParam("OutputArgumentSerializationName", "FastJsonNet")
				.WithHeader("Content-Type", "application/mrpc+json; charset=utf-8")
				.WithHeader("X-Auth-Token", AccessManagementTestConstants.AuthTokenValue)
				.UsingPost())
			.RespondWith(Response.Create()
				.WithHeader("Content-Type", "application/mrpc+json; charset=utf-8")
				.WithBody(serializedResponse)
				.WithStatusCode(HttpStatusCode.OK));
	}

	public Task DisposeAsync()
	{
		_server?.Stop();
		_server?.Dispose();
		return Task.CompletedTask;
	}
}
