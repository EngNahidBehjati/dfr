using DataFlowRunner.DataFlowExecution.Models;

namespace DataFlowRunner.DataFlowSynchronization.Abstraction;

internal interface IDataFlowActionLockExecutor
{
	void Invoke(DataFlowExecutionId dataFlowExecutionId, string actionKey, Action action,
		CancellationToken cancellationToken = default);

	Task InvokeAsync(DataFlowExecutionId dataFlowExecutionId, string actionKey, Func<Task> action,
		CancellationToken cancellationToken);
}
