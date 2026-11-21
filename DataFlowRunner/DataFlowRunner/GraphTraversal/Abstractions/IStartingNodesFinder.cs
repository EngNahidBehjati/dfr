using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.GraphTraversal.Abstractions;

internal interface IStartingNodesFinder
{
	IReadOnlySet<NodeId> FindStartingNodes(IReadOnlyGraph graph);
}
