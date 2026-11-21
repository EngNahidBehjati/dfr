using System.Net;
using System.Net.Http.Json;
using DataFlowRunner.ApplicationInformation.Models;
using DataFlowRunner.Integration.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DataFlowRunner.Integration.Tests.ApplicationInformation.Controllers;

public sealed class ApplicationVersionControllerTests
{
	private readonly HttpClient _httpClient;

	public ApplicationVersionControllerTests()
	{
		WebApplicationFactory<Program> factory = WebApplicationFactoryCreator.Create();

		_httpClient = factory.CreateClient();
	}

	[Fact]
	public async Task GetVersion_ShouldReturnVersionInFourParts_WhenApiCalled()
	{
		// Act
		HttpResponseMessage response = await _httpClient.GetAsync("api/version");
		ApplicationVersionDto? result = await response.Content.ReadFromJsonAsync<ApplicationVersionDto>();

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		result.Should().NotBeNull();
		result.ServiceVersion.Should().MatchRegex(@"^\d+\.\d+\.\d+\.\d+$");
	}
}
