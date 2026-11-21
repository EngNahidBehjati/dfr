using System.Collections.Concurrent;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowAuthorization.Exceptions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.Language.LanguageDictionary;

namespace DataFlowRunner.DataFlowAuthorization.Business;

internal sealed class DataFlowExecutionUserIdStore : IDataFlowExecutionUserIdStore
{
	private readonly ConcurrentDictionary<DataFlowExecutionId, UserId> _userIds = new();
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public DataFlowExecutionUserIdStore(ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public UserId GetDataFlowExecutionUserId(DataFlowExecutionId dataFlowExecutionId)
	{
		if (!_userIds.TryGetValue(dataFlowExecutionId, out UserId userId))
		{
			throw new DataFlowExecutionUserIdNotFoundException(
				_languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.DataFlowAuthorization
					.DataFlowExecutionNotFoundOrAccessDenied(dataFlowExecutionId.Value));
		}
		return userId;
	}

	public bool TryGetDataFlowExecutionUserId(DataFlowExecutionId dataFlowExecutionId, out UserId userId)
	{
		return _userIds.TryGetValue(dataFlowExecutionId, out userId);
	}

	public void StoreDataFlowExecutionUserId(DataFlowExecutionId dataFlowExecutionId, UserId userId)
	{
		_userIds[dataFlowExecutionId] = userId;
	}
}
