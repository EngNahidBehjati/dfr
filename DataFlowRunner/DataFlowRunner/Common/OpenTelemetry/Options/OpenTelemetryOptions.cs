namespace DataFlowRunner.Common.OpenTelemetry.Options;

internal sealed class OpenTelemetryOptions
{
	public const string SectionPath = "OpenTelemetry";

	public const string DefaultOtlpEndpoint = "http://localhost:4317";

	public bool Enabled { get; init; }

	public string OtlpEndpoint { get; init; } = DefaultOtlpEndpoint;

	public string ServiceName { get; init; } = "DataFlowRunner";
}
