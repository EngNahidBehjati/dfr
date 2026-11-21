using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.GraphCore.Abstractions;

internal interface IGraph : IReadOnlyGraph
{
	void AddNodes(IReadOnlyCollection<NodeId> nodes);
	void RemoveNodes(IReadOnlyCollection<NodeId> nodeIds);
	void AddEdges(IReadOnlyCollection<EdgeByNode> edges);
	void RemoveEdges(IReadOnlyCollection<EdgeId> edges);
}
