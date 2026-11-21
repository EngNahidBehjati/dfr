using DataFlowRunner.Common.OpenTelemetry.Options;

namespace DataFlowRunner.Common.OpenTelemetry;

internal static class OpenTelemetryOptionsProvider
{
	public static OpenTelemetryOptions Read(IConfiguration configuration)
	{
		return configuration
			.GetSection(OpenTelemetryOptions.SectionPath)
			.Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();
	}
}
