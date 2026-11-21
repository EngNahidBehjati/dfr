using DataFlowRunner.Common.DataFlowBuilderConfiguration.Options;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace DataFlowRunner.Tests.Common.DataFlowBuilderConfiguration.Options;

public sealed class DataFlowBuilderEndpointOptionValidationTests
{
	private readonly DataFlowBuilderEndpointOptionValidation _sut = new();

	[Fact]
	public void Validate_ShouldFail_WhenBaseUrlIsEmpty()
	{
		var options = new DataFlowBuilderEndpoint
		{
			BaseUrl = ""
		};

		ValidateOptionsResult result = _sut.Validate(null, options);

		result.Failed.Should().BeTrue();
		result.FailureMessage.Should().Contain("BaseUrl must not be empty");
	}

	[Fact]
	public void Validate_ShouldFail_WhenBaseUrlIsInvalid()
	{
		var options = new DataFlowBuilderEndpoint
		{
			BaseUrl = "not-a-url"
		};

		ValidateOptionsResult result = _sut.Validate(null, options);

		result.Failed.Should().BeTrue();
		result.FailureMessage.Should().Contain("BaseUrl must be a valid absolute URI");
	}

	[Fact]
	public void Validate_ShouldSucceed_WhenOptionsAreValid()
	{
		var options = new DataFlowBuilderEndpoint
		{
			BaseUrl = "https://example.com"
		};

		ValidateOptionsResult result = _sut.Validate(null, options);

		result.Failed.Should().BeFalse();
		result.FailureMessage.Should().BeNullOrEmpty();
	}
}
