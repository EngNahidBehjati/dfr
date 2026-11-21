using DataFlowRunner.Common.OpenTelemetry;
using DataFlowRunner.Common.OpenTelemetry.Options;

namespace DataFlowRunner.Tests.Common.OpenTelemetry;

public sealed class OpenTelemetryEndpointResolverTests
{
	[Fact]
	public void Resolve_ShouldPreferEnvironmentVariable_WhenSet()
	{
		var previous = Environment.GetEnvironmentVariable(OpenTelemetryEndpointResolver.EnvironmentVariableName);
		try
		{
			// Arrange
			Environment.SetEnvironmentVariable(
				OpenTelemetryEndpointResolver.EnvironmentVariableName,
				"http://collector:4317");
			var options = new OpenTelemetryOptions { OtlpEndpoint = "http://localhost:4317" };

			// Act
			var endpoint = OpenTelemetryEndpointResolver.Resolve(options);

			// Assert
			endpoint.Should().Be("http://collector:4317");
		}
		finally
		{
			Environment.SetEnvironmentVariable(OpenTelemetryEndpointResolver.EnvironmentVariableName, previous);
		}
	}

	[Fact]
	public void Resolve_ShouldUseOptionsEndpoint_WhenEnvironmentVariableIsUnset()
	{
		var previous = Environment.GetEnvironmentVariable(OpenTelemetryEndpointResolver.EnvironmentVariableName);
		try
		{
			// Arrange
			Environment.SetEnvironmentVariable(OpenTelemetryEndpointResolver.EnvironmentVariableName, null);
			var options = new OpenTelemetryOptions { OtlpEndpoint = "http://custom:4317" };

			// Act
			var endpoint = OpenTelemetryEndpointResolver.Resolve(options);

			// Assert
			endpoint.Should().Be("http://custom:4317");
		}
		finally
		{
			Environment.SetEnvironmentVariable(OpenTelemetryEndpointResolver.EnvironmentVariableName, previous);
		}
	}
}
