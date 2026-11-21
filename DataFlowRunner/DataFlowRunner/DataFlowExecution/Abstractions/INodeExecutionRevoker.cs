using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

internal interface INodeExecutionRevoker
{
	Task RevokeNodeExecution(DataFlowExecutionId dataFlowExecutionId,
		IReadOnlyCollection<NodeId> nodeIds,
		IDataFlowRevocationNodeNextState dataFlowRevocationNodeNextState,
		CancellationToken cancellationToken);
}
