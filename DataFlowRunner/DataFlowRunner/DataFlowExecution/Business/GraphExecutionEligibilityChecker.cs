using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class GraphExecutionEligibilityChecker : IGraphExecutionEligibilityChecker
{
	private readonly INodeExecutionStateStore _nodeExecutionStateStore;
	private readonly INodeExecutionStateClassifier _nodeExecutionStateClassifier;

	public GraphExecutionEligibilityChecker(INodeExecutionStateStore nodeExecutionStateStore,
		INodeExecutionStateClassifier nodeExecutionStateClassifier)
	{
		_nodeExecutionStateStore = nodeExecutionStateStore ?? throw new ArgumentNullException(nameof(nodeExecutionStateStore));
		_nodeExecutionStateClassifier = nodeExecutionStateClassifier ?? throw new ArgumentNullException(nameof(nodeExecutionStateClassifier));
	}

	public bool IsGraphExecutable(IReadOnlyGraph graph, DataFlowExecutionId dataFlowExecutionId)
	{
		return graph.GetAllNodes().All(n =>
		{
			NodeExecutionState state = _nodeExecutionStateStore.GetNodeExecutionState(dataFlowExecutionId, n.Id);
			return _nodeExecutionStateClassifier.IsRunnable(state);
		});
	}
}
