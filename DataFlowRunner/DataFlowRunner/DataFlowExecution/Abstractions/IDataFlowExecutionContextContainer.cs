using System.Diagnostics.CodeAnalysis;
using DataFlowRunner.DataFlowExecution.Models;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

internal interface IDataFlowExecutionContextContainer
{
	bool TryGetDataFlowExecutionContext(DataFlowExecutionId dataFlowExecutionId, [NotNullWhen(true)] out DataFlowExecutionContext? dataFlow);
	void AddOrUpdateDataFlowExecutionContext(DataFlowExecutionContext dataFlowExecutionContext);
}
