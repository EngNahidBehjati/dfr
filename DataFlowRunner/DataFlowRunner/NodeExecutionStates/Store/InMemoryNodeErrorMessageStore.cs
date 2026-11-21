using System.Collections.Concurrent;
using DataFlowRunner.Common.Exceptions.NotFound;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Store;

internal sealed class InMemoryNodeErrorMessageStore : INodeErrorMessageStore
{
	private readonly ConcurrentDictionary<DataFlowExecutionId, NodeErrorMessagesContainer>
		_nodeErrorMessagesByDataFlowExecutionId = new();
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public InMemoryNodeErrorMessageStore(ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public NodeErrorMessage GetNodeErrorMessage(Guid dataFlowExecutionId, Guid nodeId)
	{
		if (!_nodeErrorMessagesByDataFlowExecutionId.TryGetValue(new DataFlowExecutionId(dataFlowExecutionId),
				out NodeErrorMessagesContainer? nodeErrorMessagesContainer))
		{
			var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage
				.DataFlowRunner.DataFlowExecution.DataFlowExecutionNotFound();
			throw new DataFlowExecutionNotFoundException(errorMessage);
		}

		return nodeErrorMessagesContainer.GetMessage(new NodeId(nodeId));
	}

	public void SetNodeErrorMessage(Guid dataFlowExecutionId, Guid nodeId, NodeErrorMessage message)
	{
		NodeErrorMessagesContainer container = _nodeErrorMessagesByDataFlowExecutionId.GetOrAdd(
			new DataFlowExecutionId(dataFlowExecutionId),
			_ => new NodeErrorMessagesContainer(_languageDictionaryProvider));

		container.SetMessage(new NodeId(nodeId), message);
	}
}
