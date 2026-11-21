using DataFlowRunner.Common.Authorization.Models;
using Microsoft.Extensions.Options;

namespace DataFlowRunner.Common.Authorization.Options;

internal sealed class DiaRegistrationConfigsOptionValidation : IValidateOptions<DiaRegistrationConfigs>
{
	public ValidateOptionsResult Validate(string? name, DiaRegistrationConfigs options)
	{
		ArgumentNullException.ThrowIfNull(options);

		if (options.TargetSystemId == Guid.Empty)
		{
			return ValidateOptionsResult.Fail("DiaTargetSystemId must be configured and not empty.");
		}

		return ValidateOptionsResult.Success;
	}
}
