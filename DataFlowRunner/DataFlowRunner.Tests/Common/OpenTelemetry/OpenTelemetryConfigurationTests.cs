using DataFlowRunner.Common.OpenTelemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace DataFlowRunner.Tests.Common.OpenTelemetry;

public sealed class OpenTelemetryConfigurationTests
{
	[Fact]
	public void ConfigureOpenTelemetry_ShouldNotRegisterTracerProvider_WhenDisabled()
	{
		// Arrange
		WebApplicationBuilder builder = WebApplication.CreateBuilder();
		builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
		{
			["OpenTelemetry:Enabled"] = "false"
		});

		// Act
		builder.ConfigureOpenTelemetry();

		// Assert
		using WebApplication app = builder.Build();
		app.Services.GetService<TracerProvider>().Should().BeNull();
	}

	[Fact]
	public void ConfigureOpenTelemetry_ShouldNotRegisterMeterProvider_WhenDisabled()
	{
		// Arrange
		WebApplicationBuilder builder = WebApplication.CreateBuilder();
		builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
		{
			["OpenTelemetry:Enabled"] = "false"
		});

		// Act
		builder.ConfigureOpenTelemetry();

		// Assert
		using WebApplication app = builder.Build();
		app.Services.GetService<MeterProvider>().Should().BeNull();
	}

	[Fact]
	public void ConfigureOpenTelemetry_ShouldRegisterTracerProvider_WhenEnabled()
	{
		// Arrange
		WebApplicationBuilder builder = WebApplication.CreateBuilder();
		builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
		{
			["OpenTelemetry:Enabled"] = "true",
			["OpenTelemetry:OtlpEndpoint"] = "http://localhost:4317",
			["OpenTelemetry:ServiceName"] = "DataFlowRunner.Tests"
		});

		// Act
		builder.ConfigureOpenTelemetry();

		// Assert
		using WebApplication app = builder.Build();
		app.Services.GetService<TracerProvider>().Should().NotBeNull();
	}

	[Fact]
	public void ConfigureOpenTelemetry_ShouldRegisterMeterProvider_WhenEnabled()
	{
		// Arrange
		WebApplicationBuilder builder = WebApplication.CreateBuilder();
		builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
		{
			["OpenTelemetry:Enabled"] = "true",
			["OpenTelemetry:OtlpEndpoint"] = "http://localhost:4317",
			["OpenTelemetry:ServiceName"] = "DataFlowRunner.Tests"
		});

		// Act
		builder.ConfigureOpenTelemetry();

		// Assert
		using WebApplication app = builder.Build();
		app.Services.GetService<MeterProvider>().Should().NotBeNull();
	}
}
