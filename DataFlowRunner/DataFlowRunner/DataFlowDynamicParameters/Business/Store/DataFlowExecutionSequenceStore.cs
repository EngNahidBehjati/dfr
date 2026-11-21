using System.Collections.Concurrent;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowDynamicParameters.Business.Store;

internal sealed class DataFlowExecutionSequenceStore : IDataFlowExecutionSequenceStore
{
	private readonly ConcurrentDictionary<DataFlowExecutionId, ExecutionSequenceState> _executionStatesById = new();

	public void SetNextRunSequence(DataFlowExecutionId executionId, IReadOnlyCollection<NodeId> nodes)
	{
		ExecutionSequenceState state = _executionStatesById.GetOrAdd(executionId, static _ => new ExecutionSequenceState());
		var sequence = state.NextSequence();

		foreach (NodeId nodeId in nodes)
			state.SetNodeRunSequence(nodeId, sequence);
	}

	public long GetNodeRunSequence(DataFlowExecutionId executionId, NodeId nodeId)
	{
		return _executionStatesById.TryGetValue(executionId, out ExecutionSequenceState? state)
		       && state.TryGetNodeRunSequence(nodeId, out var sequence)
			? sequence
			: 0;
	}
}
