using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowAuthorization.Exceptions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.Language.LanguageDictionary;

namespace DataFlowRunner.DataFlowAuthorization.Business;

internal sealed class RouteParameterExtractor : IRouteParameterExtractor
{
	private const string DataFlowExecutionId = "dataFlowExecutionId";
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public RouteParameterExtractor(ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_languageDictionaryProvider = languageDictionaryProvider ??
		                              throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}


	public DataFlowExecutionId ExtractDataFlowExecutionId(HttpContext context)
	{
		ArgumentNullException.ThrowIfNull(context);

		var dataFlowExecutionIdNotFoundErrorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage
			.DataFlowRunner.DataFlowAuthorization.DataFlowExecutionIdNotFoundInRoute();

		if (!context.Request.RouteValues.TryGetValue(DataFlowExecutionId, out var dataFlowExecutionId) ||
		    dataFlowExecutionId == null)
		{
			throw new DataFlowExecutionIdNotFoundInRouteException(dataFlowExecutionIdNotFoundErrorMessage);
		}

		return Guid.TryParse(dataFlowExecutionId.ToString(),
			out Guid parsedDataFlowExecutionId)
			? new DataFlowExecutionId(parsedDataFlowExecutionId)
			: throw new DataFlowExecutionIdNotFoundInRouteException(dataFlowExecutionIdNotFoundErrorMessage);
	}
}
