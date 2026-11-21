using DataFlowRunner.DataFlowExecution.Models;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

internal interface INodeExecutorEngineGraphInitializer
{
	Task TryInitialize(DataFlowExecutionId dataFlowExecutionId,
		CancellationToken cancellationToken = default);
}
