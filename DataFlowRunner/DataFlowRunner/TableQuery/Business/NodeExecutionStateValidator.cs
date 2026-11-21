using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;
using DataFlowRunner.TableQuery.Abstractions;
using DataFlowRunner.TableQuery.Exceptions;

namespace DataFlowRunner.TableQuery.Business;

internal sealed class NodeExecutionStateValidator : INodeExecutionStateValidator
{
	private readonly INodeExecutionStateProvider _nodeExecutionStateProvider;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public NodeExecutionStateValidator(INodeExecutionStateProvider nodeExecutionStateProvider,
		ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_nodeExecutionStateProvider = nodeExecutionStateProvider
		                              ?? throw new ArgumentNullException(nameof(nodeExecutionStateProvider));
		_languageDictionaryProvider = languageDictionaryProvider
		                              ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public void ValidateNodeIsSucceeded(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId)
	{
		NodeExecutionState nodeState = _nodeExecutionStateProvider.GetNodeExecutionState(dataFlowExecutionId, nodeId);

		if (nodeState != NodeExecutionState.Succeeded)
		{
			var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage
				.DataFlowRunner.TableQuery.NodeExecutionStateNotSucceeded();
			throw new NodeExecutionStateNotSucceededException(errorMessage);
		}
	}
}
