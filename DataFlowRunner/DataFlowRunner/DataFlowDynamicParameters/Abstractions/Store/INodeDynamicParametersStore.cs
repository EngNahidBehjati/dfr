using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;

internal interface INodeDynamicParametersStore
{
	List<DynamicParameter> GetDynamicParameters(NodeId nodeId);
	void SetDynamicParameters(NodeId nodeId, List<DynamicParameter> dynamicParameters);
}
