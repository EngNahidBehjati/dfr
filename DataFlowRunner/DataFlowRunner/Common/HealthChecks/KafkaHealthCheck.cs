using Confluent.Kafka;
using DataFlowRunner.Common.WolverineConfiguration.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace DataFlowRunner.Common.HealthChecks
{
    internal class KafkaHealthCheck : IHealthCheck
    {
        private readonly KafkaBootstrap _kafkaBootstrap;

        public KafkaHealthCheck(IOptions<KafkaBootstrap> kafkaBootstrap)
        {
			ArgumentNullException.ThrowIfNull(kafkaBootstrap);
			_kafkaBootstrap = kafkaBootstrap.Value;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var config = new AdminClientConfig { BootstrapServers = _kafkaBootstrap.Address };
            try
            {
                using IAdminClient? adminClient = new AdminClientBuilder(config).Build();
                Metadata? meta = adminClient.GetMetadata(TimeSpan.FromSeconds(3));
                if (meta.Brokers.Count > 0)
                    return Task.FromResult(HealthCheckResult.Healthy("Kafka is healthy"));
                return Task.FromResult(HealthCheckResult.Unhealthy("Kafka broker not found"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Kafka health check failed", ex));
            }
        }
    }
}
