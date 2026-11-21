using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

internal interface IDataFlowExecutor
{
	Task RunDataFlowAsync(DataFlowExecutionContext dataFlowExecutionContext,
		List<DynamicParameter> dynamicParameters,
		CancellationToken cancellationToken);

	Task RunNodeAsync(DataFlowExecutionContext dataFlowExecutionContext, NodeId nodeId,
		List<DynamicParameter> dynamicParameters, CancellationToken cancellationToken);
}
