
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowSnapshot.Abstractions;
using DataFlowRunner.DataFlowSnapshot.Models;
using FluentAssertions;
using Refit;
using RichardSzalay.MockHttp;

namespace DataFlowRunner.Tests.DataFlowSnapshot.Abstractions;

public class DataFlowTopologyClientApiTests
{
	private const string AuthenticationCookie = "675b24f1-a5db-430c-9f2c-ea4607621f5b";

	[Fact]
	public async Task GetDataFlowTopology_ShouldSendRequestAndDeserializeResponse_Whenever()
	{
		// Arrange
		var pipelineId = Guid.NewGuid();
		const long dataFlowVersion = 12;
		var authenticationCookie = new AuthenticationCookie($"Auth-Token={AuthenticationCookie}");
		const string baseUrl = "https://api.example.com";
		MockHttpMessageHandler mockHttp =
			GetMockedHttpMessageHandler(baseUrl, pipelineId, dataFlowVersion);
		var httpClient = mockHttp.ToHttpClient();
		httpClient.BaseAddress = new Uri(baseUrl);
		IDataFlowTopologyClientApi sut = RestService.For<IDataFlowTopologyClientApi>(httpClient);

		var expected = new DataFlowTopologyDto(
			new TopologyDto(
				[
					new NodeDto(
						new Guid("e1185264-5717-4565-b3fc-2c963f36af12"),
						"MANUAL_TABLE",
						[new PortDto(new Guid("f1185264-5717-4565-b3fc-2c963f36a11f"), PortDirectionDto.Out)],
						Version: 1),
					new NodeDto(
						new Guid("a27b5e2d-1d88-4157-be83-603601d7b310"),
						"TEMP_STORE",
						[
							new PortDto(new Guid("7cd7f8d4-4cd0-4111-af57-d78efef797a8"), PortDirectionDto.In),
							new PortDto(new Guid("94695f81-815e-441c-9eeb-3124101ba25d"), PortDirectionDto.Out)
						],
						Version: 1)
				],
				[
					new EdgeDto(
						new Guid("2b63f910-ae77-4a94-a491-83501e70b3e6"),
						new Guid("f1185264-5717-4565-b3fc-2c963f36a11f"),
						new Guid("7cd7f8d4-4cd0-4111-af57-d78efef797a8"),
						new Guid("e1185264-5717-4565-b3fc-2c963f36af12"),
						new Guid("a27b5e2d-1d88-4157-be83-603601d7b310"))
				],
				new ProcessorInfoDto("SYSTEM_DEFAULT_PROCESSOR", null),
				new ExecutionSettingsDto("PIPELINE_EXECUTOR", null)),
			DataFlowVersion: dataFlowVersion);

		// Act
		DataFlowTopologyDto response = await sut.GetDataFlowTopologyAsync(
			pipelineId,
			authenticationCookie,
			dataFlowVersion,
			CancellationToken.None);

		// Assert
		mockHttp.VerifyNoOutstandingExpectation();
		response.Should().BeEquivalentTo(expected);
	}

	private static MockHttpMessageHandler GetMockedHttpMessageHandler(
		string baseurl,
		Guid pipelineId,
		long dataFlowVersion)
	{
		const string expectedResponseJson = """
		                                    {
		                                      "sessionId": "ef2c3ef1-99ea-425c-8aee-25c2c2186d75",
		                                      "version": 12,
		                                      "metadata": {
		                                        "title": "new pipeline",
		                                        "pipelineType": "NORMAL"
		                                      },
		                                      "topology": {
		                                        "plugins": [
		                                          {
		                                            "type": "MANUAL_TABLE",
		                                            "version": 1,
		                                            "id": "e1185264-5717-4565-b3fc-2c963f36af12",
		                                            "title": "manual table",
		                                            "ports": [
		                                              {
		                                                "id": "f1185264-5717-4565-b3fc-2c963f36a11f",
		                                                "type": "OUT",
		                                                "multiple": false,
		                                                "title": "manual table",
		                                                "validity": "VALID"
		                                              }
		                                            ],
		                                            "representation": {
		                                              "x": "189",
		                                              "y": "54"
		                                            },
		                                            "hasWarning": false
		                                          },
		                                          {
		                                            "type": "TEMP_STORE",
		                                            "version": 1,
		                                            "id": "a27b5e2d-1d88-4157-be83-603601d7b310",
		                                            "title": "temporary store",
		                                            "ports": [
		                                              {
		                                                "id": "7cd7f8d4-4cd0-4111-af57-d78efef797a8",
		                                                "type": "IN",
		                                                "multiple": false,
		                                                "title": "input data",
		                                                "validity": "VALID"
		                                              },
		                                              {
		                                                "id": "94695f81-815e-441c-9eeb-3124101ba25d",
		                                                "type": "OUT",
		                                                "multiple": false,
		                                                "title": "temporary store",
		                                                "validity": "VALID"
		                                              }
		                                            ],
		                                            "representation": {
		                                              "x": 15,
		                                              "y": 190
		                                            },
		                                            "hasWarning": false
		                                          }
		                                        ],
		                                        "links": [
		                                          {
		                                            "id": "2b63f910-ae77-4a94-a491-83501e70b3e6",
		                                            "from": "f1185264-5717-4565-b3fc-2c963f36a11f",
		                                            "to": "7cd7f8d4-4cd0-4111-af57-d78efef797a8",
		                                            "fromPlugin": "e1185264-5717-4565-b3fc-2c963f36af12",
		                                            "toPlugin": "a27b5e2d-1d88-4157-be83-603601d7b310"
		                                          }
		                                        ],
		                                        "executionSettings": {
		                                          "type": "PIPELINE_EXECUTOR"
		                                        },
		                                        "dataDestination": {
		                                          "type": "SYSTEM_DEFAULT_PROCESSOR"
		                                        }
		                                      }
		                                    }
		                                    """;

		var mockHttp = new MockHttpMessageHandler();
		mockHttp.Expect(HttpMethod.Get, $"{baseurl}/api/v2/pipelines/{pipelineId}/topology?version={dataFlowVersion}")
			.WithHeaders("Accept", "application/json")
			.WithHeaders("Cookie", $"Auth-Token={AuthenticationCookie}")
			.Respond("application/json", expectedResponseJson);
		return mockHttp;
	}
}
