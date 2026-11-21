using DataFlowRunner.Common.OpenTelemetry;
using DataFlowRunner.Common.OpenTelemetry.Options;
using Microsoft.Extensions.Configuration;

namespace DataFlowRunner.Tests.Common.OpenTelemetry;

public sealed class OpenTelemetryOptionsProviderTests
{
	[Fact]
	public void Read_ShouldBindFromConfiguration_WhenSectionIsPresent()
	{
		// Arrange
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["OpenTelemetry:Enabled"] = "true",
				["OpenTelemetry:OtlpEndpoint"] = "http://otel:4317",
				["OpenTelemetry:ServiceName"] = "CustomService"
			})
			.Build();

		// Act
		OpenTelemetryOptions options = OpenTelemetryOptionsProvider.Read(configuration);

		// Assert
		options.Enabled.Should().BeTrue();
		options.OtlpEndpoint.Should().Be("http://otel:4317");
		options.ServiceName.Should().Be("CustomService");
	}
}
