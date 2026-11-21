using DataFlowRunner.ExecutionManagement.Options;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace DataFlowRunner.Tests.ExecutionManagement.Options;

public sealed class RetryExecutionInvokeTimeoutOptionValidatorTest
{
	private readonly RetryExecutionInvokeTimeoutOptionValidator _sut = new();

	[Theory]
	[InlineData(0)]
	[InlineData(-10)]
	public void Validate_ShouldFail_WhenRetryExecutionInvokeTimeoutInSecondsIsLowerThan1(int seconds)
	{
		// Arrange
		var options = new RetryExecutionInvokeTimeoutOptions
		{
			TimeoutInSeconds = TimeSpan.FromSeconds(seconds)
		};

		// Act
		ValidateOptionsResult actual = _sut.Validate("", options);

		// Assert
		actual.Failed.Should().BeTrue();
		actual.Failures.Should().ContainSingle()
			.Which.Should().Be("RetryExecutionInvokeTimeoutInSeconds must be greater than zero.");
	}

	[Fact]
	public void Validate_ShouldSucceed_WhenRetryExecutionInvokeTimeoutInSecondsIsGreaterThan0()
	{
		// Arrange
		var options = new RetryExecutionInvokeTimeoutOptions
		{
			TimeoutInSeconds = TimeSpan.FromSeconds(10)
		};

		// Act
		ValidateOptionsResult actual = _sut.Validate("", options);

		// Assert
		actual.Failed.Should().BeFalse();
		actual.FailureMessage.Should().BeNullOrEmpty();
	}
}
