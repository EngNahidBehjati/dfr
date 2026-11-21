using DataFlowRunner.Common.DataFlowBuilderConfiguration.Options;
using DataFlowRunner.Common.Http.Constants;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace DataFlowRunner.Common.HealthChecks
{
	internal class DataFlowBuilderHealthCheck : IHealthCheck
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly DataFlowBuilderEndpoint _dataFlowBuilderEndpoint;

		public DataFlowBuilderHealthCheck(IHttpClientFactory httpClientFactory,
			IOptions<DataFlowBuilderEndpoint> dataFlowBuilderEndpoint)
		{
			ArgumentNullException.ThrowIfNull(dataFlowBuilderEndpoint);
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_dataFlowBuilderEndpoint = dataFlowBuilderEndpoint.Value;
		}

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
			CancellationToken cancellationToken = default)
		{
			var healthUrl = new Uri(new Uri(_dataFlowBuilderEndpoint.BaseUrl), "actuator/health");
			try
			{
				HttpClient client = _httpClientFactory.CreateClient(HttpClientNames.DataFlowBuilder);
				HttpResponseMessage response =
					await client.GetAsync(healthUrl, cancellationToken).ConfigureAwait(false);
				if (response.IsSuccessStatusCode)
					return HealthCheckResult.Healthy("Data Flow Builder is healthy");
				return HealthCheckResult.Unhealthy(await ReadBodyAsync(response.Content).ConfigureAwait(false));
			}
			catch (Exception ex)
			{
				return HealthCheckResult.Unhealthy("Data Flow Builder health check failed", ex);
			}
		}

		private static async Task<string?> ReadBodyAsync(HttpContent? content)
		{
			if (content is null)
			{
				return null;
			}

			try
			{
				return await content.ReadAsStringAsync().ConfigureAwait(false);
			}
			catch (Exception exception)
			{
				return $"<ERROR reading body: {exception.Message}>";
			}
		}
	}
}
