using DataFlowRunner.DataFlowExecution.Models;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

internal interface IDataFlowDisposer
{
	Task DisposeAsync(DataFlowExecutionId dataFlowExecutionId, CancellationToken cancellationToken = default);
}
