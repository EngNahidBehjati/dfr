using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;

internal interface IDynamicParametersStoreUpdater
{
	void UpdateDataFlowDynamicParameters(
		DataFlowExecutionId executionId,
		IReadOnlyDictionary<NodeId, List<DynamicParameter>> parametersByNode);
}
