using Microsoft.Extensions.Options;

namespace DataFlowRunner.TableQuery.Options;

internal sealed class TableQueryInvokeTimeoutOptionValidation : IValidateOptions<TableQueryInvokeTimeoutOptions>
{
	public ValidateOptionsResult Validate(string? name, TableQueryInvokeTimeoutOptions options)
	{
		if (options.TimeoutInSeconds <= TimeSpan.Zero)
		{
			return ValidateOptionsResult.Fail(
				"TableQueryInvokeTimeoutInSeconds must be greater than zero.");
		}
		return ValidateOptionsResult.Success;
	}
}
