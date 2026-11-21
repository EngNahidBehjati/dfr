using DataFlowRunner.DataFlowChangeEvents.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;

namespace DataFlowRunner.DataFlowChangeEvents.Business.Handlers;

internal sealed class AddNodeHandler : IDataFlowChangeEventHandler<AddNode>
{
	private readonly INodeExecutionStateUpdater _nodeExecutionStateUpdater;

	public AddNodeHandler(INodeExecutionStateUpdater nodeExecutionStateUpdater)
	{
		_nodeExecutionStateUpdater = nodeExecutionStateUpdater ?? throw new ArgumentNullException(nameof(nodeExecutionStateUpdater));
	}

	public Task HandleAsync(DataFlowChangeContext context,
		AddNode @event,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(@event);

		var nodeId = new NodeId(@event.NodeInfo.Id);
		context.Graph.AddNodes([nodeId]);
		_nodeExecutionStateUpdater.TryAddNodeExecutionState(context.DataFlowExecutionId, nodeId);

		return Task.CompletedTask;
	}
}
