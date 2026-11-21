using Microsoft.Extensions.Options;

namespace DataFlowRunner.Common.DataFlowBuilderConfiguration.Options;

public class DataFlowBuilderEndpointOptionValidation : IValidateOptions<DataFlowBuilderEndpoint>
{
	public ValidateOptionsResult Validate(string? name, DataFlowBuilderEndpoint options)
	{
		ArgumentNullException.ThrowIfNull(options);

#pragma warning disable CA1062
		if (string.IsNullOrWhiteSpace(options.BaseUrl))
#pragma warning restore CA1062
		{
			return ValidateOptionsResult.Fail("BaseUrl must not be empty.");
		}

		return !Uri.TryCreate(options.BaseUrl,
			UriKind.Absolute,
			out _)
			? ValidateOptionsResult.Fail("BaseUrl must be a valid absolute URI.")
			: ValidateOptionsResult.Success;
	}
}
