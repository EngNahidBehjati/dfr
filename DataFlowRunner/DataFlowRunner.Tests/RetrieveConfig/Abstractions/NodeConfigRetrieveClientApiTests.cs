using System.Text.Json;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowNodeConfigs.Abstractions;
using DataFlowRunner.DataFlowNodeConfigs.Models;
using Refit;
using RichardSzalay.MockHttp;

namespace DataFlowRunner.Tests.RetrieveConfig.Abstractions;

public class NodeConfigRetrieveClientApiTests
{
	private const string AuthenticationCookie = "a4ee023f-241d-4030-b165-476603157ed1";

	[Fact]
	public async Task RetrieveNodeConfigsAsync_ShouldSendCorrectRequestAndDeserializeResponse_Whenever()
	{
		// Arrange
		var pipelineId = Guid.NewGuid();
		var authenticationCookie = new AuthenticationCookie($"Auth-Token={AuthenticationCookie}");
		var requestDataFlowNodeConfigsDto = new RequestDateFlowNodeConfigsDto(
		[
			new RequestNodeConfigDto(Guid.NewGuid(), "10"),
			new RequestNodeConfigDto(Guid.NewGuid(), "12")
		]);
		const string baseUrl = "https://api.example.com";
		MockHttpMessageHandler mockHttp =
			GetMockedHttpMessageHandler(requestDataFlowNodeConfigsDto, baseUrl, pipelineId);
		var httpClient = mockHttp.ToHttpClient();
		httpClient.BaseAddress = new Uri(baseUrl);
		INodeConfigRetrieveClientApi sut = RestService.For<INodeConfigRetrieveClientApi>(httpClient);

		// Act
		ResponseDataFlowNodeConfigsDto response =
			await sut.RetrieveNodeConfigsAsync(pipelineId, authenticationCookie, requestDataFlowNodeConfigsDto,
				CancellationToken.None);

		// Assert
		mockHttp.VerifyNoOutstandingExpectation();
		await Verify(response);
	}

	private static MockHttpMessageHandler GetMockedHttpMessageHandler(
		RequestDateFlowNodeConfigsDto requestDateFlowNodeConfigsDto,
		string baseurl,
		Guid pipelineId)
	{
		var serialized = JsonSerializer.Serialize(requestDateFlowNodeConfigsDto);

		const string expectedResponseJson = """
		                                    {
		                                    "exeConfigList": [
		                                    {
		                                      "pluginId": "878d6ee0-e86e-48fa-9022-a4c4690e51c1",
		                                      "version": "98765",
		                                      "pluginExeJson": {
		                                    "$type": "MicroService.DIP.ScenarioDesignerContract.DataContracts.PluginCommand, ETLEngineContract",
		                                    "PluginTitle": "اجرای کد دستوری SQL",
		                                    "PluginLabel": "اجرای کد دستوری SQL",
		                                    "PluginId": "20e64905-49e1-11f0-b432-08002744de90",
		                                    "Content" : {
		                                    "variable" : "4"
		                                    }
		                                    }
		                                    }
		                                    ]
		                                    }
		                                    """;

		var mockHttp = new MockHttpMessageHandler();
		mockHttp.Expect(HttpMethod.Post, $"{baseurl}/api/v1/state/{pipelineId}/config")
			.WithHeaders("Accept", "application/json")
			.WithHeaders("Cookie", $"Auth-Token={AuthenticationCookie}")
			.WithContent(serialized)
			.Respond("application/json", expectedResponseJson);
		return mockHttp;
	}
}
