using DataFlowRunner.DataFlowChangeEvents.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;
using DataFlowRunner.DataFlowCleanup.Abstraction;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowChangeEvents.Business.Handlers;

internal sealed class RemoveNodeHandler : IDataFlowChangeEventHandler<RemoveNode>
{
	private readonly IDataFlowInvalidator _dataFlowInvalidator;
	private readonly INodeRemover _nodeRemover;

	public RemoveNodeHandler(IDataFlowInvalidator dataFlowInvalidator, INodeRemover nodeRemover)
	{
		_dataFlowInvalidator = dataFlowInvalidator ?? throw new ArgumentNullException(nameof(dataFlowInvalidator));
		_nodeRemover = nodeRemover ?? throw new ArgumentNullException(nameof(nodeRemover));
	}

	public Task HandleAsync(DataFlowChangeContext context,
		RemoveNode @event,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(@event);

		InvalidateDownstreamNodes(@event, context, cancellationToken);
		_nodeRemover.RemoveNodesAsync(context.DataFlowExecutionId, [new NodeId(@event.Id)], cancellationToken);

		return Task.CompletedTask;
	}

	private void InvalidateDownstreamNodes(RemoveNode @event, DataFlowChangeContext context, CancellationToken cancellationToken)
	{
		var nodeId = new NodeId(@event.Id);
		if (!context.Graph.NodeExists(nodeId)) return;

		_dataFlowInvalidator.InvalidateNodeAndDownstreamAsync(
			context.ToRunningExecutionContext(),
			nodeId,
			cancellationToken);
	}
}
