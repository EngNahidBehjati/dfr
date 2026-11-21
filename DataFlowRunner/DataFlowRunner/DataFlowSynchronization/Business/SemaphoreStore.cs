using System.Collections.Concurrent;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowSynchronization.Abstraction;

namespace DataFlowRunner.DataFlowSynchronization.Business;

internal sealed class SemaphoreStore : ISemaphoreStore
{
	private readonly ConcurrentDictionary<DataFlowExecutionId,
		ConcurrentDictionary<string, SemaphoreSlim>> _semaphores = new();

	public SemaphoreSlim GetSemaphore(DataFlowExecutionId dataFlowExecutionId, string actionKey)
	{
		ConcurrentDictionary<string, SemaphoreSlim> dataFlowSemaphores =
			_semaphores.GetOrAdd(dataFlowExecutionId, _ => new ConcurrentDictionary<string, SemaphoreSlim>());

		SemaphoreSlim actionSemaphore = dataFlowSemaphores.GetOrAdd(actionKey,
			_ => new SemaphoreSlim(1, 1));

		return actionSemaphore;
	}

	public void RemoveDataFlowSemaphores(DataFlowExecutionId dataFlowExecutionId)
	{
		_semaphores.Remove(dataFlowExecutionId, out _);
	}
}
