using DataFlowRunner.DataFlowDynamicParameters.Abstractions.ProvideRequiredDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowDynamicParameters.Business.ProvideRequiredDynamicParameters;

internal sealed class NodeAncestryParameterValueExtractor : INodeAncestryParameterValueExtractor
{
	private readonly IDataFlowDynamicParametersStore _dataFlowDynamicParametersStore;
	private readonly IDataFlowExecutionSequenceStore _dataFlowExecutionSequenceStore;

	public NodeAncestryParameterValueExtractor(
		IDataFlowDynamicParametersStore dataFlowDynamicParametersStore,
		IDataFlowExecutionSequenceStore dataFlowExecutionSequenceStore)
	{
		_dataFlowDynamicParametersStore = dataFlowDynamicParametersStore ??
		                                  throw new ArgumentNullException(nameof(dataFlowDynamicParametersStore));
		_dataFlowExecutionSequenceStore = dataFlowExecutionSequenceStore ??
		                                 throw new ArgumentNullException(nameof(dataFlowExecutionSequenceStore));
	}

	public IReadOnlyCollection<DynamicParameter> Extract(
		DataFlowExecutionId executionId,
		NodeId nodeId,
		IReadOnlyGraph graph,
		IReadOnlyCollection<string> expectedParameterNames)
	{
		Dictionary<NodeId, List<NodeId>> parentsByNode = BuildParentLookup(graph.GetAllEdges());

		var resolvedParameters = new Dictionary<string, DynamicParameter>();
		var visitedNodes = new HashSet<NodeId> { nodeId };
		var currentLevelNodes = new List<NodeId> { nodeId };
		var remainingParameters = new HashSet<string>(expectedParameterNames);

		while (currentLevelNodes.Count > 0
		       && remainingParameters.Count > 0)
		{
			List<DynamicParameter> currentLevelResolvedParameters = GetResolvedLevelParameters(executionId,
				currentLevelNodes, resolvedParameters);

			UpdateResolvedParameters(currentLevelResolvedParameters, resolvedParameters);

			remainingParameters.ExceptWith(currentLevelResolvedParameters.Select(p => p.Name));

			currentLevelNodes = BuildNextLevel(currentLevelNodes, parentsByNode, visitedNodes);
		}

		return resolvedParameters.Values.ToList();
	}

	private static void UpdateResolvedParameters(List<DynamicParameter> currentLevelResolvedParameters,
		Dictionary<string, DynamicParameter> resolvedParameters)
	{
		foreach (DynamicParameter parameter in currentLevelResolvedParameters)
		{
			resolvedParameters[parameter.Name] = parameter;
		}
	}

	private List<DynamicParameter> GetResolvedLevelParameters(
		DataFlowExecutionId executionId,
		IReadOnlyCollection<NodeId> level,
		IReadOnlyDictionary<string, DynamicParameter> resolved)
	{
		var levelCandidates = new Dictionary<string, (DynamicParameter Parameter, long Sequence)>();

		foreach (NodeId nodeId in level)
		{
			var sequence = _dataFlowExecutionSequenceStore.GetNodeRunSequence(executionId, nodeId);

			if (!_dataFlowDynamicParametersStore.TryGetDynamicParameters(executionId, nodeId,
				    out IReadOnlyCollection<DynamicParameter> parameters))
			{
				continue;
			}

			foreach (DynamicParameter parameter in parameters)
			{
				if (resolved.ContainsKey(parameter.Name))
					continue;

				if (!levelCandidates.TryGetValue(parameter.Name,
					    out (DynamicParameter Parameter, long Sequence) existing)
				    || sequence > existing.Sequence)
					levelCandidates[parameter.Name] = (parameter, sequence);
			}
		}

		return levelCandidates.Select(c => c.Value.Parameter).ToList();
	}

	private static List<NodeId> BuildNextLevel(
		IReadOnlyCollection<NodeId> currentLevel,
		IReadOnlyDictionary<NodeId, List<NodeId>> parentsByNode,
		HashSet<NodeId> visited)
	{
		var nextLevel = new List<NodeId>();

		foreach (NodeId levelNodeId in currentLevel)
		{
			if (!parentsByNode.TryGetValue(levelNodeId, out List<NodeId>? parents))
				continue;

			foreach (NodeId parent in parents)
				if (visited.Add(parent))
					nextLevel.Add(parent);
		}

		return nextLevel;
	}

	private static Dictionary<NodeId, List<NodeId>> BuildParentLookup(
		IReadOnlyCollection<EdgeByNode> edges)
	{
		return edges
			.GroupBy(edge => edge.ToNodeId)
			.ToDictionary(
				group => group.Key,
				group => group.Select(edge => edge.FromNodeId).ToList());
	}
}
