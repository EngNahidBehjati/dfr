using DataFlowRunner.DataFlowChangeEvents.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.GraphCore.Models.EdgeModels;

namespace DataFlowRunner.DataFlowChangeEvents.Business.Handlers;

internal sealed class RemoveEdgeHandler : IDataFlowChangeEventHandler<RemoveEdge>
{
	private readonly IDataFlowInvalidator _dataFlowInvalidator;

	public RemoveEdgeHandler(
		IDataFlowInvalidator dataFlowInvalidator)
	{
		_dataFlowInvalidator = dataFlowInvalidator ?? throw new ArgumentNullException(nameof(dataFlowInvalidator));
	}

	public Task HandleAsync(DataFlowChangeContext context, RemoveEdge @event, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(@event);

		InvalidateDownstreamNodes(@event, context, cancellationToken);

		context.Graph.RemoveEdges([new EdgeId(@event.Id)]);
		return Task.CompletedTask;
	}

	private void InvalidateDownstreamNodes(RemoveEdge @event, DataFlowChangeContext context, CancellationToken cancellationToken)
	{
		EdgeByNode? edge = context.Graph.GetEdges([new EdgeId(@event.Id)]).FirstOrDefault();
		if (edge == null) return;

		_dataFlowInvalidator.InvalidateNodeAndDownstreamAsync(
			context.ToRunningExecutionContext(), edge.ToNodeId, cancellationToken);
	}
}
