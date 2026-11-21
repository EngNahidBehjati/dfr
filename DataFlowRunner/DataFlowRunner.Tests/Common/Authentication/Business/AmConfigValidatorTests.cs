using DataFlowRunner.Common.Authentication.Business;
using DataFlowRunner.Common.Authentication.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace DataFlowRunner.Tests.Common.Authentication.Business;

public class AmConfigValidatorTests
{
	private readonly AmConfigValidator _sut = new();

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData(" \t")]
	public void Validate_ShouldReturnFailed_WhenEngineVersionIsNullOrWhiteSpace(string? engineVersion)
	{
		// Arrange
		var amConfig = new AmConfig
		{
			BaseUrl = new Uri("https://example.com"),
			EngineVersion = engineVersion!
		};

		// Act
		ValidateOptionsResult result = _sut.Validate(null, amConfig);

		// Assert
		result.Failed.Should().BeTrue();
		result.Failures.Should().ContainSingle()
			.Which.Should().Be("Am Config EngineVersion: Engine version must be configured and not empty.");
	}

	[Fact]
	public void Validate_ShouldReturnSucceeded_WhenEngineVersionIsProvided()
	{
		// Arrange
		var amConfig = new AmConfig
		{
			BaseUrl = new Uri("https://example.com"),
			EngineVersion = "1.0.0"
		};

		// Act
		ValidateOptionsResult result = _sut.Validate(null, amConfig);

		// Assert
		result.Succeeded.Should().BeTrue();
	}
}
