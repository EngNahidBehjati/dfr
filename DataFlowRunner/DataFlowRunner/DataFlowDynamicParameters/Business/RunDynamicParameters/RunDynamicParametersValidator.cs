using DataFlowRunner.DataFlowDynamicParameters.Abstractions.RunDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Exceptions;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.Language.LanguageDictionary;

namespace DataFlowRunner.DataFlowDynamicParameters.Business.RunDynamicParameters;

internal sealed class RunDynamicParametersValidator : IRunDynamicParametersValidator
{
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public RunDynamicParametersValidator(ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_languageDictionaryProvider = languageDictionaryProvider
		                              ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public void EnsureProvidedParametersAreValid(
		ResponseDataFlowDynamicParametersDto fetchResponse,
		IReadOnlyCollection<DynamicParameter> providedParameters)
	{
		List<ResponseDataFlowDynamicParameterDto> fetchedParameters = GetFetchedParameters(fetchResponse);
		EnsureAllParametersExists(fetchedParameters, providedParameters);
		EnsureRequiredParameterValuesAreNotNull(fetchedParameters, providedParameters);
	}

	private static List<ResponseDataFlowDynamicParameterDto> GetFetchedParameters(
		ResponseDataFlowDynamicParametersDto fetchResponse)
	{
		return fetchResponse.NodeDynamicParameters
			.SelectMany(plugin => plugin.DynamicParameters)
			.DistinctBy(parameter => parameter.Id)
			.ToList();
	}

	private void EnsureAllParametersExists(
		List<ResponseDataFlowDynamicParameterDto> fetchedParameters,
		IReadOnlyCollection<DynamicParameter> providedParameters)
	{
		var missingParameterNames = fetchedParameters
			.Select(parameter => parameter.Name)
			.Except(providedParameters.Select(parameter => parameter.Name))
			.ToList();

		if (missingParameterNames.Count == 0) return;

		var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner
			.DynamicParameters.DynamicParametersNotFilled(GetParameterNamesString(missingParameterNames));

		throw new DynamicParameterNotFilledException(errorMessage);
	}

	private void EnsureRequiredParameterValuesAreNotNull(
		List<ResponseDataFlowDynamicParameterDto> fetchedParameters,
		IReadOnlyCollection<DynamicParameter> providedParameters)
	{
		var providedByName = providedParameters.ToDictionary(parameter => parameter.Name);
		var nullRequiredNames = fetchedParameters
			.Where(parameter => parameter.Required)
			.Where(parameter => providedByName[parameter.Name].Value is null)
			.Select(parameter => parameter.Name)
			.Distinct()
			.ToList();

		if (nullRequiredNames.Count == 0) return;

		var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner
			.DynamicParameters.RequiredDynamicParameterValueIsNull(GetParameterNamesString(nullRequiredNames));

		throw new DynamicParameterNotFilledException(errorMessage);
	}

	private static string GetParameterNamesString(List<string> parameterNames)
	{
		return string.Join(", ", parameterNames.Select(param => $"\"{param}\""));
	}
}
