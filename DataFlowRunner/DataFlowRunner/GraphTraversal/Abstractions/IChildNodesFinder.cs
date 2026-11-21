using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.GraphTraversal.Abstractions;

internal interface IChildNodesFinder
{
	IReadOnlySet<NodeId> GetChildNodeIds(NodeId nodeId, IReadOnlyGraph graph);
}
