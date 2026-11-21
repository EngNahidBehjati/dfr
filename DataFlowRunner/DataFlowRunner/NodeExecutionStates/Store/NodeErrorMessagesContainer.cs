using System.Collections.Concurrent;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Exceptions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Store;

internal sealed class NodeErrorMessagesContainer
{
	private readonly ConcurrentDictionary<NodeId, NodeErrorMessage> _errorMessages = new();
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public NodeErrorMessagesContainer(ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_languageDictionaryProvider = languageDictionaryProvider
		                              ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public NodeErrorMessage GetMessage(NodeId nodeId)
	{
		if (!_errorMessages.TryGetValue(nodeId, out NodeErrorMessage? message))
		{
			var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage
				.DataFlowRunner.NodeExecutionStates.NodeErrorMessageNotFound();
			throw new NodeErrorMessageNotFoundException(errorMessage);
		}

		return message;
	}

	public void SetMessage(NodeId nodeId, NodeErrorMessage message)
	{
		_errorMessages[nodeId] = message;
	}
}
