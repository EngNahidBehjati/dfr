using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowSynchronization.Abstraction;

namespace DataFlowRunner.DataFlowSynchronization.Business;

internal sealed class DataFlowActionLockExecutor : IDataFlowActionLockExecutor
{
	private readonly ISemaphoreStore _semaphoreStore;

	public DataFlowActionLockExecutor(ISemaphoreStore semaphoreStore)
	{
		_semaphoreStore = semaphoreStore ?? throw new ArgumentNullException(nameof(semaphoreStore));
	}

	public void Invoke(DataFlowExecutionId dataFlowExecutionId, string actionKey, Action action,
		CancellationToken cancellationToken = default)
	{
		SemaphoreSlim semaphore = _semaphoreStore.GetSemaphore(dataFlowExecutionId, actionKey);
		semaphore.Wait(cancellationToken);
		try
		{
			action();
		}
		finally
		{
			semaphore.Release();
		}
	}

	public async Task InvokeAsync(DataFlowExecutionId dataFlowExecutionId,
		string actionKey, Func<Task> action, CancellationToken cancellationToken)
	{
		SemaphoreSlim semaphore = _semaphoreStore.GetSemaphore(dataFlowExecutionId, actionKey);
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			await action().ConfigureAwait(false);
		}
		finally
		{
			semaphore.Release();
		}
	}
}
