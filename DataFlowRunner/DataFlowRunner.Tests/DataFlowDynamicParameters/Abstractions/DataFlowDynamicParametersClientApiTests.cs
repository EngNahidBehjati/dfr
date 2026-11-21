using System.Text.Json;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Infrastructure;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using FluentAssertions;
using Refit;
using RichardSzalay.MockHttp;

namespace DataFlowRunner.Tests.DataFlowDynamicParameters.Abstractions;

public class DataFlowDynamicParametersClientApiTests
{
	private const string AuthenticationCookie = "a4ee023f-241d-4030-b165-476603157ed1";

	[Fact]
	public async Task GetDataFlowDynamicParameters_ShouldSendRequestAndDeserializeResponse_Whenever()
	{
		// Arrange
		var pipelineId = Guid.NewGuid();
		var authenticationCookie = new AuthenticationCookie($"Auth-Token={AuthenticationCookie}");
		var requestDataFlowDynamicParametersDto = new RequestDataFlowDynamicParametersDto([]);
		const string baseUrl = "https://api.example.com";
		MockHttpMessageHandler mockHttp =
			GetMockedHttpMessageHandler(requestDataFlowDynamicParametersDto, baseUrl, pipelineId);
		var httpClient = mockHttp.ToHttpClient();
		httpClient.BaseAddress = new Uri(baseUrl);
		IDataFlowDynamicParametersClientApi sut = RestService.For<IDataFlowDynamicParametersClientApi>(httpClient);
		var pluginId = new Guid("11185264-5717-4565-b3fc-2c963f36af11");
		var expected = new ResponseDataFlowDynamicParametersDto(
		[
			new ResponseNodeDynamicParametersDto(
				pluginId,
				[
					new ResponseDataFlowDynamicParameterDto(
						"26ecfeb5-367f-48ec-b980-b56cff3b6d66",
						"age_parameter",
						false,
						"INTEGER",
						false,
						1
					)
				])
		]);

		// Act
		ResponseDataFlowDynamicParametersDto response =
			await sut.GetDataFlowDynamicParametersAsync(pipelineId, authenticationCookie,
				requestDataFlowDynamicParametersDto,
				CancellationToken.None);

		// Assert
		mockHttp.VerifyNoOutstandingExpectation();
		response.Should().BeEquivalentTo(expected);
	}

	private static MockHttpMessageHandler GetMockedHttpMessageHandler(
		RequestDataFlowDynamicParametersDto requestDataFlowDynamicParametersDto,
		string baseurl,
		Guid pipelineId)
	{
		var serialized = JsonSerializer.Serialize(requestDataFlowDynamicParametersDto);

		const string expectedResponseJson = """
		                                    {
		                                      "pluginVariablesMap": [
		                                        {
		                                          "pluginId": "11185264-5717-4565-b3fc-2c963f36af11",
		                                          "variablesList": [
		                                            {
		                                              "id": "26ecfeb5-367f-48ec-b980-b56cff3b6d66",
		                                              "name": "age_parameter",
		                                              "dataType": "INTEGER",
		                                              "list": false,
		                                              "required": false,
		                                              "defaultValue": 1
		                                            }
		                                          ]
		                                        }
		                                      ]
		                                    }
		                                    """;

		var mockHttp = new MockHttpMessageHandler();
		mockHttp.Expect(HttpMethod.Post, $"{baseurl}/api/v1/pipelines/{pipelineId}/simple-variables")
			.WithHeaders("Accept", "application/json")
			.WithHeaders("Cookie", $"Auth-Token={AuthenticationCookie}")
			.WithContent(serialized)
			.Respond("application/json", expectedResponseJson);
		return mockHttp;
	}
}
