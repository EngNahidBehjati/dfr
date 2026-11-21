using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.Common.Http.Constants;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace DataFlowRunner.Common.HealthChecks
{
    internal class AmHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AmConfig _amConfig;

        public AmHealthCheck(IHttpClientFactory httpClientFactory, IOptions<AmConfig> amConfig)
        {
			ArgumentNullException.ThrowIfNull(amConfig);
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _amConfig = amConfig.Value;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var healthUrl = new Uri(_amConfig.BaseUrl, "api/health");
            try
            {
                HttpClient client = _httpClientFactory.CreateClient(HttpClientNames.AccessManagement);
                HttpResponseMessage response = await client.GetAsync(healthUrl, cancellationToken).ConfigureAwait(false);
                return response.IsSuccessStatusCode ? HealthCheckResult.Healthy("AM is healthy") : HealthCheckResult.Unhealthy(await ReadBodyAsync(response.Content).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("AM health check failed", ex);
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
