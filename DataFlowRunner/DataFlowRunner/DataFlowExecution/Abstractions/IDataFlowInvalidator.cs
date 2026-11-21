using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

internal interface IDataFlowInvalidator
{
	Task InvalidateDataFlowAsync(DataFlowExecutionContext dataFlowExecutionContext, CancellationToken cancellationToken);

	Task InvalidateNodeAndDownstreamAsync(DataFlowExecutionContext dataFlowExecutionContext, NodeId sourceNodeId,
		CancellationToken cancellationToken);
}
