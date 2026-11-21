using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Business;

internal sealed class RunnableNodesFinder : IRunnableNodesFinder
{
	private readonly INodeExecutionStateProvider _nodeExecutionStateProvider;
	private readonly INodeExecutionStateClassifier _nodeExecutionStateClassifier;

	public RunnableNodesFinder(INodeExecutionStateProvider nodeExecutionStateProvider,
		INodeExecutionStateClassifier nodeExecutionStateClassifier)
	{
		_nodeExecutionStateProvider = nodeExecutionStateProvider ?? throw new ArgumentNullException(nameof(nodeExecutionStateProvider));
		_nodeExecutionStateClassifier = nodeExecutionStateClassifier ?? throw new ArgumentNullException(nameof(nodeExecutionStateClassifier));
	}

	public List<Node> FindRunnableNodes(IReadOnlyGraph graph, DataFlowExecutionId dataFlowExecutionId)
	{
		return graph.GetAllNodes().Where(n =>
		{
			NodeExecutionState state = _nodeExecutionStateProvider.GetNodeExecutionState(dataFlowExecutionId, n.Id);
			return _nodeExecutionStateClassifier.IsRunnable(state);
		}).ToList();
	}
}
