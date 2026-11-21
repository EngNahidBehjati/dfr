using System.Collections.Concurrent;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;

namespace DataFlowRunner.DataFlowExecution.Business
{
	internal sealed class NodeExecutorEngineGraphInitializationTracker : INodeExecutorEngineGraphInitializationTracker
	{
		private readonly ConcurrentDictionary<DataFlowExecutionId, bool> _initializedDataFlowExecutions = new();

		public void TrackDataFlowInitialization(DataFlowExecutionId dataFlowExecutionId)
		{
			_initializedDataFlowExecutions.TryAdd(dataFlowExecutionId, true);
		}

		public void UntrackDataFlowInitialization(DataFlowExecutionId dataFlowExecutionId)
		{
			_initializedDataFlowExecutions.TryRemove(dataFlowExecutionId, out _);
		}

		public bool IsDataFlowInitialized(DataFlowExecutionId dataFlowExecutionId)
		{
			return _initializedDataFlowExecutions.ContainsKey(dataFlowExecutionId);
		}
	}
}
