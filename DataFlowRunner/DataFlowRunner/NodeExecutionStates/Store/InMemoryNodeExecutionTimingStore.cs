using System.Collections.Concurrent;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Exceptions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Store;

internal sealed class InMemoryNodeExecutionTimingStore : INodeExecutionTimingStore
{
	private readonly ConcurrentDictionary<DataFlowExecutionId, NodeExecutionTimingContainer> _nodeExecutionTime = new();
	private readonly TimeProvider _timeProvider;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public InMemoryNodeExecutionTimingStore(TimeProvider timeProvider,
		ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public NodeExecutionTiming GetNodeExecutionTime(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId)
	{
		return _nodeExecutionTime.TryGetValue(dataFlowExecutionId, out NodeExecutionTimingContainer? nodeExecutionTime)
			? nodeExecutionTime.GetNodeExecutionTime(nodeId)
			: new NodeExecutionTiming();
	}

	public void SetNodeExecutionStartTime(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId)
	{
		NodeExecutionTimingContainer nodeTimingsContainer = _nodeExecutionTime.GetOrAdd(
			dataFlowExecutionId,
			_ => new NodeExecutionTimingContainer(_languageDictionaryProvider));

		nodeTimingsContainer.SetNodeExecutionStartTime(nodeId, GetNow());
	}

	public void SetNodeExecutionEndTime(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId)
	{
		if (_nodeExecutionTime.TryGetValue(dataFlowExecutionId,
			    out NodeExecutionTimingContainer? nodeExecutionTimeContainer))
		{
			nodeExecutionTimeContainer.SetNodeExecutionEndTime(nodeId, GetNow());
		}
		else
		{
			var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner
				.NodeExecutionStates.NodeExecutionStartTimeNotSet();
			throw new NodeExecutionStartTimeNotSetException(errorMessage);
		}
	}

	private DateTimeOffset GetNow()
	{
		return _timeProvider.GetUtcNow();
	}
}
