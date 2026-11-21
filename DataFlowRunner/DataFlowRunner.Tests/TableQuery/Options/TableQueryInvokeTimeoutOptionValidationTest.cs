using DataFlowRunner.TableQuery.Options;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace DataFlowRunner.Tests.TableQuery.Options;

public sealed class TableQueryInvokeTimeoutOptionValidationTest
{
	private readonly TableQueryInvokeTimeoutOptionValidation _sut = new();

	[Theory]
	[InlineData(0)]
	[InlineData(-10)]
	public void Validate_ShouldFail_WhenTableQueryInvokeTimeoutInSecondsIsLowerThan1(int seconds)
	{
		// Arrange
		var tableQueryInvokeTimeoutOptions = new TableQueryInvokeTimeoutOptions
		{
			TimeoutInSeconds = TimeSpan.FromSeconds(seconds)
		};

		// Act
		ValidateOptionsResult actual = _sut.Validate("", tableQueryInvokeTimeoutOptions);

		// Assert
		actual.Failed.Should().BeTrue();
		actual.Failures.Should().ContainSingle()
			.Which.Should().Be("TableQueryInvokeTimeoutInSeconds must be greater than zero.");
	}

	[Fact]
	public void Validate_ShouldSucceed_WhenTableQueryInvokeTimeoutInSecondsIsGreaterThen0()
	{
		// Arrange
		var tableQueryInvokeTimeoutOptions = new TableQueryInvokeTimeoutOptions
		{
			TimeoutInSeconds = TimeSpan.FromSeconds(10)
		};

		// Act
		ValidateOptionsResult actual = _sut.Validate("", tableQueryInvokeTimeoutOptions);

		// Assert
		actual.Failed.Should().BeFalse();
		actual.FailureMessage.Should().BeNullOrEmpty();
	}
}
