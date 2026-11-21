using DataFlowRunner.DataFlowChangeEvents.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowChangeEvents.Business.Handlers;

internal sealed class AddEdgeHandler : IDataFlowChangeEventHandler<AddEdge>
{
	private readonly IDataFlowInvalidator _dataFlowInvalidator;

	public AddEdgeHandler(IDataFlowInvalidator dataFlowInvalidator)
	{
		_dataFlowInvalidator = dataFlowInvalidator ?? throw new ArgumentNullException(nameof(dataFlowInvalidator));
	}

	public Task HandleAsync(DataFlowChangeContext context,
		AddEdge @event,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(@event);

		var toNodeId = new NodeId(@event.EdgeInfo.ToNode);
		_dataFlowInvalidator.InvalidateNodeAndDownstreamAsync(
			context.ToRunningExecutionContext(), toNodeId, cancellationToken);

		var edge = new EdgeByNode
		{
			Id = new EdgeId(@event.EdgeInfo.Id),
			FromNodeId = new NodeId(@event.EdgeInfo.FromNode),
			ToNodeId = toNodeId
		};
		context.Graph.AddEdges([edge]);

		return Task.CompletedTask;
	}
}
