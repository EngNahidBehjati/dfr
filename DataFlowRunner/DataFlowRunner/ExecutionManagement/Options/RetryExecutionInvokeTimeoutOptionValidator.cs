using Microsoft.Extensions.Options;

namespace DataFlowRunner.ExecutionManagement.Options;

internal sealed class RetryExecutionInvokeTimeoutOptionValidator : IValidateOptions<RetryExecutionInvokeTimeoutOptions>
{
	public ValidateOptionsResult Validate(string? name, RetryExecutionInvokeTimeoutOptions options)
	{
		if (options.TimeoutInSeconds <= TimeSpan.Zero)
		{
			return ValidateOptionsResult.Fail(
				$"{RetryExecutionInvokeTimeoutOptions.TimeoutKey} must be greater than zero.");
		}

		return ValidateOptionsResult.Success;
	}
}
