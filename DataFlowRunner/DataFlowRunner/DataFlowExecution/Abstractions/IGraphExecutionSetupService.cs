using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

internal interface IGraphExecutionSetupService
{
	Task SetupExecution(IReadOnlyGraph graph,
		DataFlowExecutionId dataFlowExecutionId,
		CancellationToken cancellationToken);
}
