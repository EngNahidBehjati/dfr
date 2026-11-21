using DataFlowRunner.Common.Authentication.Abstractions;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowAuthorization.Exceptions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.Language.LanguageDictionary;

namespace DataFlowRunner.DataFlowAuthorization.Business;

internal sealed class DataFlowExecutionUserAuthorizer : IDataFlowExecutionUserAuthorizer
{
	private readonly IDataFlowExecutionUserIdStore _dataFlowExecutionUserIdStore;
	private readonly IDataFlowExecutionAuthorizerClient _dataFlowExecutionAuthorizerClient;
	private readonly IAuthenticatedUserProvider _authenticatedUserProvider;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public DataFlowExecutionUserAuthorizer(
		IDataFlowExecutionUserIdStore dataFlowExecutionUserIdStore,
		IDataFlowExecutionAuthorizerClient dataFlowExecutionAuthorizerClient,
		IAuthenticatedUserProvider authenticatedUserProvider, ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_dataFlowExecutionUserIdStore = dataFlowExecutionUserIdStore ??
		                                throw new ArgumentNullException(nameof(dataFlowExecutionUserIdStore));
		_dataFlowExecutionAuthorizerClient = dataFlowExecutionAuthorizerClient ??
		                                     throw new ArgumentNullException(nameof(dataFlowExecutionAuthorizerClient));
		_authenticatedUserProvider = authenticatedUserProvider ??
		                             throw new ArgumentNullException(nameof(authenticatedUserProvider));
		_languageDictionaryProvider = languageDictionaryProvider ??
		                              throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public async Task AuthorizeUserAccessAsync(
		DataFlowExecutionId dataFlowExecutionId,
		CancellationToken cancellationToken)
	{
		UserId currentUserId = await _authenticatedUserProvider
			.GetCurrentUserId(cancellationToken).ConfigureAwait(false);

		if (_dataFlowExecutionUserIdStore.TryGetDataFlowExecutionUserId(
			    dataFlowExecutionId, out UserId dataFlowExecutionUserId))
		{
			ValidateUser(dataFlowExecutionId, currentUserId, dataFlowExecutionUserId);
			return;
		}

		await _dataFlowExecutionAuthorizerClient.AuthorizeDataFlowExecutionAsync(dataFlowExecutionId, cancellationToken)
			.ConfigureAwait(false);
		_dataFlowExecutionUserIdStore.StoreDataFlowExecutionUserId(dataFlowExecutionId, currentUserId);
	}

	private void ValidateUser(DataFlowExecutionId dataFlowExecutionId,
		UserId currentUserId,
		UserId existingUserId)
	{
		if (existingUserId == currentUserId) return;

		throw new DataFlowExecutionUserIdConflictException(
			_languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.DataFlowAuthorization
			.DataFlowExecutionNotFoundOrAccessDenied(dataFlowExecutionId.Value));
	}
}
