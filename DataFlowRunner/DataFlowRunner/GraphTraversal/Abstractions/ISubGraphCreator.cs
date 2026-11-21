using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.GraphTraversal.Abstractions;

internal interface ISubGraphCreator
{
	IReadOnlyGraph CreateSubGraphFromNodes(IReadOnlyGraph source, IReadOnlyCollection<NodeId> nodeIds);
}
