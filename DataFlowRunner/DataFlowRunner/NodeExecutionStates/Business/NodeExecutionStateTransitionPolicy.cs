using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Exceptions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Business;

internal sealed class NodeExecutionStateTransitionPolicy : INodeExecutionStateTransitionPolicy
{
	private readonly INodeExecutionStateStore _nodeExecutionStateStore;
	private readonly INodeExecutionStateMachine _nodeExecutionStateMachine;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public NodeExecutionStateTransitionPolicy(INodeExecutionStateStore nodeExecutionStateStore,
		INodeExecutionStateMachine nodeExecutionStateMachine,
		ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_nodeExecutionStateStore = nodeExecutionStateStore
		                           ?? throw new ArgumentNullException(nameof(nodeExecutionStateStore));
		_nodeExecutionStateMachine = nodeExecutionStateMachine
		                             ?? throw new ArgumentNullException(nameof(nodeExecutionStateMachine));
		_languageDictionaryProvider = languageDictionaryProvider
		                              ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public void EnsureTransitionAllowed(DataFlowExecutionId dataFlowExecutionId,
		NodeId nodeId,
		NodeExecutionState state)
	{
		NodeExecutionState currentState = _nodeExecutionStateStore.GetNodeExecutionState(dataFlowExecutionId, nodeId);
		if (_nodeExecutionStateMachine.CanTransition(currentState, state)) return;

		var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner
			.NodeExecutionStates.NodeExecutionStateTransitionNotAllowed(currentState, state);
		throw new NodeExecutionStateTransitionNotAllowedException(errorMessage);
	}
}
