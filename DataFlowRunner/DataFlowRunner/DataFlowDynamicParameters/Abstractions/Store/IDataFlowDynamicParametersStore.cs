using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;

internal interface IDataFlowDynamicParametersStore
{
	List<DynamicParameter> GetDynamicParameters(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId);
	bool TryGetDynamicParameters(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId,
		out IReadOnlyCollection<DynamicParameter> parameters);
	void SetDynamicParameters(DataFlowExecutionId dataFlowExecutionId,
		IReadOnlyCollection<NodeId> nodeIds,
		List<DynamicParameter> dynamicParameters);
}
