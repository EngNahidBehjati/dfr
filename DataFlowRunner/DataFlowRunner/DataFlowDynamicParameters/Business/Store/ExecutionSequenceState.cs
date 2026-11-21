using System.Collections.Concurrent;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowDynamicParameters.Business.Store;

internal sealed class ExecutionSequenceState
{
	private long _lastRunSequence;
	private readonly ConcurrentDictionary<NodeId, long> _nodeLastRunSequences = new();

	public long NextSequence()
	{
		return Interlocked.Increment(ref _lastRunSequence);
	}

	public void SetNodeRunSequence(NodeId nodeId, long sequence)
	{
		_nodeLastRunSequences[nodeId] = sequence;
	}

	public bool TryGetNodeRunSequence(NodeId nodeId, out long lastRunSequence)
	{
		return _nodeLastRunSequences.TryGetValue(nodeId, out lastRunSequence);
	}
}
