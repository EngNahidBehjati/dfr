using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

internal interface IDynamicParameterMismatchNodeInvalidator
{
	Task InvalidateNodesOnParameterMismatchAsync(
		DataFlowExecutionContext dataFlowExecutionContext,
		IReadOnlyGraph subgraphToCompare,
		List<DynamicParameter> providedDynamicParameters,
		CancellationToken cancellationToken);
}
