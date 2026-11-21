using System.Collections.Concurrent;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Store;

internal sealed class InMemoryDataFlowExecutionStatusStore : IDataFlowExecutionStatusStore
{
	private readonly ConcurrentDictionary<DataFlowExecutionId, DataFlowExecutionStatus> _states = new();

	public DataFlowExecutionStatus Get(DataFlowExecutionId executionId)
	{
		return _states.GetOrAdd(executionId, _ => DataFlowExecutionStatus.DefaultStatus);
	}

	public void Set(DataFlowExecutionId executionId, DataFlowExecutionStatus state)
	{
		_states[executionId] = state;
	}
}
