using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowDynamicParameters.Abstractions.ProvideRequiredDynamicParameters;

internal interface INodeAncestryParameterValueExtractor
{
	/// <summary>
	/// Extracts the effective parameter set for <paramref name="nodeId"/> by walking the graph
	/// upward, level by level (BFS). The closest ancestor that holds a value for a given parameter
	/// name wins. When multiple nodes at the same level carry different values for the same name,
	/// the node with the highest submission version (i.e. most recently submitted) wins;
	/// </summary>
	/// <param name="expectedParameterNames">
	/// The set of parameter names the caller needs. When non-empty, the BFS stops as soon as every
	/// name in this set has been resolved, avoiding unnecessary traversal of distant ancestors.
	/// Pass an empty collection to collect all parameters from the full ancestry tree.
	/// </param>
	IReadOnlyCollection<DynamicParameter> Extract(
		DataFlowExecutionId executionId,
		NodeId nodeId,
		IReadOnlyGraph graph,
		IReadOnlyCollection<string> expectedParameterNames);
}
