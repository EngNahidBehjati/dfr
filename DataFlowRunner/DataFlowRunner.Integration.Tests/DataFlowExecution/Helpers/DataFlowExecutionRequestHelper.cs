using System.Text;
using System.Text.Json;
using DataFlowRunner.Integration.Tests.Common.Constants;
using DataFlowRunner.Integration.Tests.DataFlowExecution.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine.Tracking;

namespace DataFlowRunner.Integration.Tests.DataFlowExecution.Helpers;

internal sealed class DataFlowExecutionRequestHelper
{
	private const int TrackingTimeoutInSeconds = 20;
	private readonly WebApplicationFactory<Program> _factory;
	private readonly HttpClient _httpClient;

	public DataFlowExecutionRequestHelper(WebApplicationFactory<Program> factory)
	{
		_factory = factory ?? throw new ArgumentNullException(nameof(factory));
		_httpClient = _factory.CreateClient();
		_httpClient.DefaultRequestHeaders.Add("Cookie", AccessManagementTestConstants.AuthCookie);
	}

	public async Task<DataFlowExecutionResult> ExecuteRequestAsync(string requestUrl, object? body = null)
	{
		IHost host = _factory.Services.GetRequiredService<IHost>();
		var response = new HttpResponseContainer();

		ITrackedSession session = await host.Services
			.TrackActivity()
			.Timeout(TimeSpan.FromSeconds(TrackingTimeoutInSeconds))
			.ExecuteAndWaitAsync(_ => SendRequestAsync(response, requestUrl, body));

		return new DataFlowExecutionResult(response, session);
	}

	private async Task SendRequestAsync(HttpResponseContainer responseContainer, string requestUrl, object? body)
	{
		using HttpRequestMessage request = BuildPostRequest(requestUrl, body);
		HttpResponseMessage response = await _httpClient.SendAsync(request).ConfigureAwait(false);
		var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
		responseContainer.SetProperties(response, responseBody);
	}

	private static HttpRequestMessage BuildPostRequest(string requestUrl, object? body)
	{
		var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
		if (body is null)
		{
			return request;
		}

		var serializedBody = JsonSerializer.Serialize(body);
		request.Content = new StringContent(serializedBody, Encoding.UTF8, "application/json");
		return request;
	}
}
