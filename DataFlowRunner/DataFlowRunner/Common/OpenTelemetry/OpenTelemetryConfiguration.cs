using DataFlowRunner.Common.OpenTelemetry.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace DataFlowRunner.Common.OpenTelemetry;

public static class OpenTelemetryConfiguration
{
	public static void ConfigureOpenTelemetry(this WebApplicationBuilder builder)
	{
		OpenTelemetryOptions options = OpenTelemetryOptionsProvider.Read(builder.Configuration);
		if (!options.Enabled)
		{
			return;
		}

		var endpoint = OpenTelemetryEndpointResolver.Resolve(options);
		Uri otlpEndpoint = new(endpoint);

		builder.Services.AddOpenTelemetry()
			.ConfigureResource(resource => resource.AddService(options.ServiceName))
			.WithTracing(tracing => tracing
				.AddAspNetCoreInstrumentation()
				.AddHttpClientInstrumentation()
				.AddSource("Wolverine")
				.AddOtlpExporter(otlp => otlp.Endpoint = otlpEndpoint))
			.WithMetrics(metrics => metrics
				.AddAspNetCoreInstrumentation()
				.AddHttpClientInstrumentation()
				.AddRuntimeInstrumentation()
				.AddProcessInstrumentation()
				.AddMeter("Wolverine*")
				.AddOtlpExporter(otlp => otlp.Endpoint = otlpEndpoint));
	}
}
