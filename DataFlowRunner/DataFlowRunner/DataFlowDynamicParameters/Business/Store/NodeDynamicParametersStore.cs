using System.Collections.Concurrent;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowDynamicParameters.Business.Store;

internal sealed class NodeDynamicParametersStore : INodeDynamicParametersStore
{
	private readonly ConcurrentDictionary<NodeId, List<DynamicParameter>> _nodeDynamicParameters = new();

	public List<DynamicParameter> GetDynamicParameters(NodeId nodeId)
	{
		return _nodeDynamicParameters.TryGetValue(nodeId,
			out List<DynamicParameter>? dynamicParameters)
			? dynamicParameters
			: [];
	}

	public void SetDynamicParameters(NodeId nodeId, List<DynamicParameter> dynamicParameters)
	{
		if (dynamicParameters.Count == 0)
		{
			_nodeDynamicParameters.TryRemove(nodeId, out List<DynamicParameter>? _);
			return;
		}

		_nodeDynamicParameters[nodeId] = dynamicParameters;
	}
}
