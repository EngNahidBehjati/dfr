using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.NodeExecutionStates.Abstractions;

internal interface IRunnableNodesFinder
{
	List<Node> FindRunnableNodes(IReadOnlyGraph graph, DataFlowExecutionId dataFlowExecutionId);
}
