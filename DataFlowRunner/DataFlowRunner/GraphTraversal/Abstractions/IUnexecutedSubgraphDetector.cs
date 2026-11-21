using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;

namespace DataFlowRunner.GraphTraversal.Abstractions;

internal interface IUnExecutedSubgraphDetector
{
	IReadOnlyGraph DetectUnExecutedSubgraph(
		IReadOnlyGraph graph, DataFlowExecutionId dataFlowExecutionId);
}
