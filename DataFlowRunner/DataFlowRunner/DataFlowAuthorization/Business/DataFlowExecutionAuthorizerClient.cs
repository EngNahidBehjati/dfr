using System.Net;
using DataFlowRunner.Common.Authentication.Abstractions;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowAuthorization.Exceptions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.Language.LanguageDictionary;
using Refit;

namespace DataFlowRunner.DataFlowAuthorization.Business;

internal sealed class DataFlowExecutionAuthorizerClient : IDataFlowExecutionAuthorizerClient
{
	private readonly IDataFlowAuthorizationClientApi _authorizationClientApi;
	private readonly IAuthenticationTokenAccessor _authenticationTokenAccessor;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public DataFlowExecutionAuthorizerClient(
		IDataFlowAuthorizationClientApi dataFlowAuthorizationClientApi,
		IAuthenticationTokenAccessor authenticationTokenAccessor,
		ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_authorizationClientApi = dataFlowAuthorizationClientApi ?? throw new ArgumentNullException(nameof(dataFlowAuthorizationClientApi));
		_authenticationTokenAccessor = authenticationTokenAccessor ?? throw new ArgumentNullException(nameof(authenticationTokenAccessor));
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public async Task AuthorizeDataFlowExecutionAsync(
		DataFlowExecutionId dataFlowExecutionId,
		CancellationToken cancellationToken)
	{
		AuthenticationCookie authenticationCookie = _authenticationTokenAccessor.GetAuthenticationCookie();

		try
		{
			await _authorizationClientApi.CheckDataFlowAuthorizationAsync(
					dataFlowExecutionId.Value,
					authenticationCookie,
					null,
					cancellationToken)
				.ConfigureAwait(false);
		}
		catch (HttpRequestException httpRequestException)
		{
			var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.DataFlowAuthorization
				.DataFlowExecutionAuthorizationFailed(dataFlowExecutionId.Value, httpRequestException.Message);
			throw new DataFlowExecutionAuthorizationFailedException(errorMessage, httpRequestException);
		}
		catch (TaskCanceledException taskCanceledException)
		{
			var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.DataFlowAuthorization
				.DataFlowExecutionAuthorizationFailed(dataFlowExecutionId.Value, taskCanceledException.Message);
			throw new DataFlowExecutionAuthorizationFailedException(errorMessage, taskCanceledException);
		}
		catch (ApiException apiException)
		{
			throw MapApiException(dataFlowExecutionId, apiException);
		}
	}

	private Exception MapApiException(DataFlowExecutionId dataFlowExecutionId, ApiException apiException)
	{
		DataFlowRunnerLanguage.DataFlowRunnerClass.DataFlowAuthorizationClass dataFlowAuthorization =
			_languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.DataFlowAuthorization;

		return apiException.StatusCode switch
		{
			HttpStatusCode.Unauthorized => new DataFlowExecutionUnauthorizedException(
				dataFlowAuthorization.DataFlowExecutionNotFoundOrAccessDenied(dataFlowExecutionId.Value)),
			HttpStatusCode.NotFound => new DataFlowExecutionNotFoundOrAccessDeniedException(
				dataFlowAuthorization.DataFlowExecutionNotFoundOrAccessDenied(dataFlowExecutionId.Value)),
			_ => new DataFlowExecutionAuthorizationFailedException(
				dataFlowAuthorization.DataFlowExecutionAuthorizationFailed(dataFlowExecutionId.Value, apiException.Message),
				apiException)
		};
	}
}
