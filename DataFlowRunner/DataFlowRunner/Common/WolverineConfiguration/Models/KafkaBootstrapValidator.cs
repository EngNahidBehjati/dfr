using Microsoft.Extensions.Options;

namespace DataFlowRunner.Common.WolverineConfiguration.Models;

internal sealed class KafkaBootstrapValidator : IValidateOptions<KafkaBootstrap>
{
	public ValidateOptionsResult Validate(string? name, KafkaBootstrap options)
	{
		if (string.IsNullOrWhiteSpace(options.Address))
		{
			return ValidateOptionsResult.Fail(
				"KafkaBootstrap: Address must be configured and not empty.");
		}

		return ValidateOptionsResult.Success;
	}
}
