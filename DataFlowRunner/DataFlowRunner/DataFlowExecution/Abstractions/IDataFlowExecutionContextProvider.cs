using System.Diagnostics.CodeAnalysis;
using DataFlowRunner.DataFlowExecution.Models;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

internal interface IDataFlowExecutionContextProvider
{
	Task UpdateDataFlowExecutionContextAsync(DataFlowExecutionId dataFlowExecutionId,
		CancellationToken cancellationToken,
		DataFlowVersion? dataFlowVersion = null);

	Task<DataFlowExecutionContext> GetRunningDataFlowExecutionContextAsync(DataFlowExecutionId dataFlowExecutionId,
		CancellationToken cancellationToken);

	bool TryGetRunningDataFlowExecutionContext(DataFlowExecutionId dataFlowExecutionId,
		[NotNullWhen(true)] out DataFlowExecutionContext? dataFlowExecutionContext);
}
