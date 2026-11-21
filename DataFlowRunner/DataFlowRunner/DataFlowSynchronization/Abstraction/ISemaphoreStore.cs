using DataFlowRunner.DataFlowExecution.Models;

namespace DataFlowRunner.DataFlowSynchronization.Abstraction;

internal interface ISemaphoreStore
{
	SemaphoreSlim GetSemaphore(DataFlowExecutionId dataFlowExecutionId, string actionKey);
	void RemoveDataFlowSemaphores(DataFlowExecutionId dataFlowExecutionId);
}
