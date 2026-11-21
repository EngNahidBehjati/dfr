using DataFlowRunner.Common.Logging.Options;
using DataFlowRunner.Common.OpenTelemetry;
using DataFlowRunner.Common.OpenTelemetry.Options;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.OpenTelemetry;

namespace DataFlowRunner.Common.Logging;

public static class LoggingConfiguration
{
	public static void ConfigureLogging(this WebApplicationBuilder builder)
	{
		LogProviderOptions logProviderOptions = builder.Configuration
			.GetSection(LogProviderOptions.SectionPath)
			.Get<LogProviderOptions>() ?? new LogProviderOptions();

		OpenTelemetryOptions openTelemetryOptions = OpenTelemetryOptionsProvider.Read(builder.Configuration);

		if (logProviderOptions.ProviderType == LogProvider.None && !openTelemetryOptions.Enabled)
		{
			return;
		}

		builder.Logging.ClearProviders();
		LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
			.ReadFrom.Configuration(builder.Configuration)
			.Enrich.FromLogContext();

		ConfigureLoggerProvider(logProviderOptions, loggerConfiguration);
		ConfigureOpenTelemetrySink(openTelemetryOptions, loggerConfiguration);

		Log.Logger = loggerConfiguration.CreateLogger();

		builder.Host.UseSerilog();
	}

	private static void ConfigureLoggerProvider(
		LogProviderOptions logProviderOptions,
		LoggerConfiguration loggerConfiguration)
	{
		if (logProviderOptions.ProviderType == LogProvider.File)
		{
			loggerConfiguration.WriteTo.File(
				new JsonFormatter(),
				logProviderOptions.EffectiveFilePath!,
				rollingInterval: RollingInterval.Day);
		}

		if (logProviderOptions.ProviderType == LogProvider.Console)
		{
			loggerConfiguration.WriteTo.Console();
		}
	}

	private static void ConfigureOpenTelemetrySink(
		OpenTelemetryOptions openTelemetryOptions,
		LoggerConfiguration loggerConfiguration)
	{
		if (!openTelemetryOptions.Enabled)
		{
			return;
		}

		string endpoint = OpenTelemetryEndpointResolver.Resolve(openTelemetryOptions);

		loggerConfiguration.WriteTo.OpenTelemetry(options =>
		{
			options.Endpoint = endpoint;
			options.Protocol = OtlpProtocol.Grpc;
			options.ResourceAttributes = new Dictionary<string, object>
			{
				["service.name"] = openTelemetryOptions.ServiceName
			};
		});
	}
}
