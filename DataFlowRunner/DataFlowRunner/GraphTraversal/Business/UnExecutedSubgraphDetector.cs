using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphTraversal.Abstractions;
using DataFlowRunner.NodeExecutionStates.Abstractions;

namespace DataFlowRunner.GraphTraversal.Business;

internal sealed class UnExecutedSubgraphDetector : IUnExecutedSubgraphDetector
{
	private readonly ISubGraphCreator _subGraphCreator;
	private readonly IRunnableNodesFinder _runnableNodesFinder;

	public UnExecutedSubgraphDetector(ISubGraphCreator subGraphCreator, IRunnableNodesFinder runnableNodesFinder)
	{
		_subGraphCreator = subGraphCreator ?? throw new ArgumentNullException(nameof(subGraphCreator));
		_runnableNodesFinder = runnableNodesFinder ?? throw new ArgumentNullException(nameof(runnableNodesFinder));
	}

	public IReadOnlyGraph DetectUnExecutedSubgraph(IReadOnlyGraph graph, DataFlowExecutionId dataFlowExecutionId)
	{
		var runnableNodeIds = _runnableNodesFinder
			.FindRunnableNodes(graph, dataFlowExecutionId)
			.Select(n => n.Id)
			.ToList();

		return _subGraphCreator.CreateSubGraphFromNodes(graph, runnableNodeIds);
	}
}
