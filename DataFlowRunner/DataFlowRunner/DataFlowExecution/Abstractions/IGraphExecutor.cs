using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

internal interface IGraphExecutor
{
	Task RunGraphAsync(IReadOnlyGraph graph,
		DataFlowExecutionId dataFlowExecutionId,
		CancellationToken cancellationToken);
}
