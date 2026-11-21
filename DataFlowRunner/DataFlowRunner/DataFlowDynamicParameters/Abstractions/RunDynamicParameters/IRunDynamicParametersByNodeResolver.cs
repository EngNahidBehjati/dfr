using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowDynamicParameters.Abstractions.RunDynamicParameters;

internal interface IRunDynamicParametersByNodeResolver
{
	Task<IReadOnlyDictionary<NodeId, List<DynamicParameter>>> ResolveProvidedParametersByNodeAsync(
		DataFlowExecutionId executionId,
		IReadOnlyCollection<Node> nodes,
		List<DynamicParameter> providedDynamicParameters,
		CancellationToken cancellationToken);
}
