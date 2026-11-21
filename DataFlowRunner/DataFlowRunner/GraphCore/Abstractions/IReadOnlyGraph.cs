using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.GraphCore.Abstractions;

internal interface IReadOnlyGraph
{
	IReadOnlyCollection<Node> GetAllNodes();
	bool NodeExists(NodeId nodeId);
	IReadOnlyCollection<EdgeByNode> GetEdges(IReadOnlyCollection<EdgeId> edges);
	IReadOnlyCollection<EdgeByNode> GetAllEdges();
}
