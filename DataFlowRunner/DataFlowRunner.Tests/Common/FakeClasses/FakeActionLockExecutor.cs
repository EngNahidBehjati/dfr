using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowSynchronization.Abstraction;

namespace DataFlowRunner.Tests.Common.FakeClasses;

internal sealed class FakeActionLockExecutor : IDataFlowActionLockExecutor
{
	public void Invoke(DataFlowExecutionId dataFlowExecutionId, string actionKey, Action action,
		CancellationToken cancellationToken = default)
	{
		action();
	}

	public async Task InvokeAsync(DataFlowExecutionId dataFlowExecutionId, string actionKey, Func<Task> action,
		CancellationToken cancellationToken)
	{
		await action();
	}
}
