using System.Net;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowSnapshot.Models;
using DataFlowRunner.Integration.Tests.Common;
using DataFlowRunner.Integration.Tests.Common.Constants;
using DataFlowRunner.Integration.Tests.Common.Wiremock;
using DataFlowRunner.Integration.Tests.DataFlowExecution.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace DataFlowRunner.Integration.Tests.DataFlowAuthorization;

public sealed class DataFlowExecutionUserAuthorizationTests : IClassFixture<DataFlowBuilderServerFixture>,
	IClassFixture<AmServerFixture>
{
	private readonly DataFlowBuilderServerFixture _dataFlowBuilderServerFixture;
	private readonly HttpClient _httpClient;

	public DataFlowExecutionUserAuthorizationTests(
		DataFlowBuilderServerFixture dataFlowBuilderServerFixture,
		AmServerFixture amServerFixture)
	{
		ArgumentNullException.ThrowIfNull(amServerFixture);

		_dataFlowBuilderServerFixture = dataFlowBuilderServerFixture ??
										throw new ArgumentNullException(nameof(dataFlowBuilderServerFixture));
		IDataFlowExecutionFacade mockDataFlowExecutionFacade = Substitute.For<IDataFlowExecutionFacade>()!;

		WebApplicationFactory<Program> factory = WebApplicationFactoryCreator.Create(
				_dataFlowBuilderServerFixture.BaseAddress,
				amServerFixture.BaseAddress,
				false)
			.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll<IDataFlowExecutionFacade>();
					services.AddSingleton(mockDataFlowExecutionFacade);
				});
			});

		_httpClient = factory.CreateClient();
		_httpClient.DefaultRequestHeaders.Add("Cookie", AccessManagementTestConstants.AuthCookie);
	}

	[Fact]
	public async Task HttpRequest_ShouldReturnOk_WhenAuthorizationApiReturnsOk()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		MockAuthorizationResponse(dataFlowExecutionId, HttpStatusCode.OK);

		// Act
		HttpResponseMessage response = await _httpClient.PostAsync(BuildInvalidateDataFlowUrl(dataFlowExecutionId), null);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Theory]
	[InlineData(HttpStatusCode.Unauthorized)]
	[InlineData(HttpStatusCode.NotFound)]
	public async Task HttpRequest_ShouldReturnSameStatusCode_WhenAuthorizationApiReturnsUnauthorizedOrNotFound(HttpStatusCode statusCode)
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		MockAuthorizationResponse(dataFlowExecutionId, statusCode);

		// Act
		HttpResponseMessage response = await _httpClient.PostAsync(BuildInvalidateDataFlowUrl(dataFlowExecutionId), null);

		// Assert
		response.StatusCode.Should().Be(statusCode);
	}

	[Fact]
	public async Task HttpRequest_ShouldReturnServiceUnavailable_WhenAuthorizationApiServiceIsUnavailable()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		MockAuthorizationResponse(dataFlowExecutionId, HttpStatusCode.ServiceUnavailable);

		// Act
		HttpResponseMessage response = await _httpClient.PostAsync(BuildInvalidateDataFlowUrl(dataFlowExecutionId), null);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
	}

	private void MockAuthorizationResponse(Guid executionId, HttpStatusCode statusCode)
	{
		object? responseBody = statusCode == HttpStatusCode.OK
			? new DataFlowTopologyDto(new TopologyDtoCreator().Create(), DataFlowVersion: 1)
			: null;

		_dataFlowBuilderServerFixture.StubJsonResponse(
			$"/api/v2/pipelines/{executionId}/topology",
			HttpMethod.Get,
			responseBody,
			(int)statusCode);
	}

	private static string BuildInvalidateDataFlowUrl(Guid executionId) =>
		$"api/data-flow-executions/{executionId}/invalidate";
}
