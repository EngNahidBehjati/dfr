using DataFlowRunner.DataFlowExecution.Models;

namespace DataFlowRunner.DataFlowSnapshot.Abstractions;

internal interface IDataFlowExecutionContextCreator
{
	Task<DataFlowExecutionContext> CreateDataFlowExecutionContextAsync(Guid requestDataFlowId,
		CancellationToken cancellationToken,
		long? version = null);
}
