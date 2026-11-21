using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using DataFlowRunner.Common.Exceptions.NotFound;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Store;

internal sealed class NodeExecutionStatesContainer
{
	private readonly ConcurrentDictionary<NodeId, NodeExecutionState> _states = new();
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public NodeExecutionStatesContainer(ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public IReadOnlyDictionary<NodeId, NodeExecutionState> GetAllStates()
	{
		return _states.ToDictionary();
	}

	public NodeExecutionState Get(NodeId nodeId)
	{
		if (!_states.TryGetValue(nodeId, out NodeExecutionState state))
		{
			var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage
				.DataFlowRunner.GraphCore.NodeNotFound();
			throw new NodeNotFoundException(errorMessage);
		}

		return state;
	}

	public bool TryGet(NodeId nodeId, [NotNullWhen(true)] out NodeExecutionState? state)
	{
		if (_states.TryGetValue(nodeId, out NodeExecutionState existingState))
		{
			state = existingState;
			return true;
		}

		state = null;
		return false;
	}

	public void Set(NodeId nodeId, NodeExecutionState state)
	{
		_states[nodeId] = state;
	}

	public void Remove(NodeId nodeId)
	{
		_states.TryRemove(nodeId, out _);
	}
}
