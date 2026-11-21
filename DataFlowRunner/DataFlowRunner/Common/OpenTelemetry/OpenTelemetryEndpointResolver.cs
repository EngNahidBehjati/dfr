using DataFlowRunner.Common.OpenTelemetry.Options;

namespace DataFlowRunner.Common.OpenTelemetry;

internal static class OpenTelemetryEndpointResolver
{
	public const string EnvironmentVariableName = "OTEL_EXPORTER_OTLP_ENDPOINT";

	public static string Resolve(OpenTelemetryOptions options)
	{
		var fromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariableName);
		if (!string.IsNullOrWhiteSpace(fromEnvironment))
		{
			return fromEnvironment;
		}

		return string.IsNullOrWhiteSpace(options.OtlpEndpoint)
			? OpenTelemetryOptions.DefaultOtlpEndpoint
			: options.OtlpEndpoint;
	}
}
