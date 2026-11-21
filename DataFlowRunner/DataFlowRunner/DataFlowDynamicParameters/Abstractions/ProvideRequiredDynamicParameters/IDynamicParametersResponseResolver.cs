using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowDynamicParameters.Abstractions.ProvideRequiredDynamicParameters;

internal interface IDynamicParametersResponseResolver
{
	/// <summary>
	/// Resolves display values for a specific node using ancestry-based resolution:
	/// the node itself is checked first, then its parents, grandparents, etc.
	/// Used for the per-node parameter API endpoint.
	/// </summary>
	List<DynamicParameterDto> ResolveForNode(
		DataFlowExecutionId executionId,
		NodeId targetNodeId,
		IReadOnlyGraph graph,
		ResponseDataFlowDynamicParametersDto dynamicParameters);

	/// <summary>
	/// Resolves display values for the overall flow with no specific target node.
	/// </summary>
	List<DynamicParameterDto> ResolveForFlow(
		DataFlowExecutionId executionId,
		IReadOnlyGraph graph,
		ResponseDataFlowDynamicParametersDto dynamicParameters);
}
