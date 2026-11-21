using DataFlowRunner.DataFlowExecution.Models;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

internal interface IDataFlowVersionConflictHandler
{
	Task HandleVersionConflictAsync(DataFlowExecutionId dataFlowExecutionId, DataFlowVersion dataFlowVersion,
		CancellationToken cancellationToken);
}
