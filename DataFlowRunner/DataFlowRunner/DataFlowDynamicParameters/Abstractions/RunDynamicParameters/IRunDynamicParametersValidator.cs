using DataFlowRunner.DataFlowDynamicParameters.Models;

namespace DataFlowRunner.DataFlowDynamicParameters.Abstractions.RunDynamicParameters;

internal interface IRunDynamicParametersValidator
{
	void EnsureProvidedParametersAreValid(
		ResponseDataFlowDynamicParametersDto fetchResponse,
		IReadOnlyCollection<DynamicParameter> providedParameters);
}
