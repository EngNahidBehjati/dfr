using System.Collections.Concurrent;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Exceptions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Store;

internal sealed class NodeExecutionTimingContainer
{
	private readonly ConcurrentDictionary<NodeId, NodeExecutionTiming> _executionTimingByNodeId = new();
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public NodeExecutionTimingContainer(ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public NodeExecutionTiming GetNodeExecutionTime(NodeId nodeId)
	{
		return _executionTimingByNodeId.TryGetValue(nodeId, out NodeExecutionTiming? nodeExecutionTime)
			? nodeExecutionTime
			: new NodeExecutionTiming();
	}

	public void SetNodeExecutionStartTime(NodeId nodeId, DateTimeOffset nodeExecutionStartTime)
	{
		_executionTimingByNodeId[nodeId] = new NodeExecutionTiming
		{
			StartedAt = nodeExecutionStartTime,
			CompletedAt = null
		};
	}

	public void SetNodeExecutionEndTime(NodeId nodeId, DateTimeOffset nodeExecutionEndTime)
	{
		if (_executionTimingByNodeId.TryGetValue(nodeId, out NodeExecutionTiming? nodeExecutionTime))
		{
			nodeExecutionTime.CompletedAt = nodeExecutionEndTime;
		}
		else
		{
			var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.NodeExecutionStates
				.NodeExecutionStartTimeNotSet();
			throw new NodeExecutionStartTimeNotSetException(errorMessage);
		}
	}
}
