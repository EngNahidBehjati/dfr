using DataFlowRunner.Common.Authentication.Models;
using Microsoft.Extensions.Options;

namespace DataFlowRunner.Common.Authentication.Business;

internal class AmConfigValidator : IValidateOptions<AmConfig>
{
	public ValidateOptionsResult Validate(string? name, AmConfig options)
	{
		if (string.IsNullOrWhiteSpace(options.EngineVersion))
		{
			return ValidateOptionsResult.Fail(
				"Am Config EngineVersion: Engine version must be configured and not empty.");
		}

		return ValidateOptionsResult.Success;
	}
}
