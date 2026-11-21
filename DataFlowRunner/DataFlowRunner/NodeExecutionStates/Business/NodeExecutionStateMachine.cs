using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Exceptions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Business;

internal sealed class NodeExecutionStateMachine : INodeExecutionStateMachine
{
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public NodeExecutionStateMachine(ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	private static readonly Dictionary<NodeExecutionState, HashSet<NodeExecutionState>> _allowedTransitions =
		new()
		{
			[NodeExecutionState.Idle] = [NodeExecutionState.Pending],
			[NodeExecutionState.Pending] =
			[
				NodeExecutionState.Running, NodeExecutionState.Failed,
				NodeExecutionState.Canceling, NodeExecutionState.Idle, NodeExecutionState.Canceled
			],
			[NodeExecutionState.Running] =
			[
				NodeExecutionState.Succeeded, NodeExecutionState.Failed, NodeExecutionState.Canceling,
				NodeExecutionState.Canceled
			],
			[NodeExecutionState.Failed] = [NodeExecutionState.Idle, NodeExecutionState.Pending],
			[NodeExecutionState.Canceling] = [NodeExecutionState.Canceled, NodeExecutionState.Failed],
			[NodeExecutionState.Canceled] = [NodeExecutionState.Idle, NodeExecutionState.Pending],
			[NodeExecutionState.Succeeded] = [NodeExecutionState.Idle]
		};

	public bool CanTransition(NodeExecutionState currentState, NodeExecutionState nextState)
	{
		if (_allowedTransitions.TryGetValue(currentState, out HashSet<NodeExecutionState>? allowedStates))
		{
			return allowedStates.Contains(nextState);
		}

		var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage
			.DataFlowRunner.NodeExecutionStates.NodeExecutionStateTransitionNotAllowed(currentState, nextState);
		throw new InvalidNodeExecutionStateException(errorMessage);
	}
}
