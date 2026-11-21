using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using DataFlowRunner.Common.Exceptions.NotFound;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Store;

internal sealed class InMemoryNodeExecutionStateStore : INodeExecutionStateStore
{
	private readonly ConcurrentDictionary<DataFlowExecutionId, NodeExecutionStatesContainer> _nodeExecutionStates = new();
	private static readonly Dictionary<NodeId, NodeExecutionState> _emptyStates = new();
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public InMemoryNodeExecutionStateStore(ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public IReadOnlyDictionary<NodeId, NodeExecutionState> GetAllNodeExecutionStates(
		DataFlowExecutionId dataFlowExecutionId)
	{
		if (_nodeExecutionStates.TryGetValue(dataFlowExecutionId, out NodeExecutionStatesContainer? nodeStates))
		{
			return nodeStates.GetAllStates();
		}

		return _emptyStates;
	}

	public NodeExecutionState GetNodeExecutionState(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId)
	{
		if (!_nodeExecutionStates.TryGetValue(dataFlowExecutionId,
				out NodeExecutionStatesContainer? nodeStates))
		{
			var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage
				.DataFlowRunner.DataFlowExecution.DataFlowExecutionNotFound();
			throw new DataFlowExecutionNotFoundException(errorMessage);
		}
		return nodeStates.Get(nodeId);
	}

	public bool TryGetNodeExecutionState(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId,
		[NotNullWhen(true)] out NodeExecutionState? state)
	{
		if (_nodeExecutionStates.TryGetValue(dataFlowExecutionId, out NodeExecutionStatesContainer? nodeStates))
		{
			return nodeStates.TryGet(nodeId, out state);
		}

		state = null;
		return false;
	}

	public void SetNodeExecutionState(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId, NodeExecutionState state)
	{
		NodeExecutionStatesContainer nodeStatesContainer = _nodeExecutionStates.GetOrAdd(
			dataFlowExecutionId,
			_ => new NodeExecutionStatesContainer(_languageDictionaryProvider));

		nodeStatesContainer.Set(nodeId, state);
	}

	public void RemoveNodeExecutionState(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId)
	{
		if (!_nodeExecutionStates.TryGetValue(dataFlowExecutionId,
			    out NodeExecutionStatesContainer? nodeStatesContainer))
		{
			return;
		}

		nodeStatesContainer.Remove(nodeId);
	}
}
