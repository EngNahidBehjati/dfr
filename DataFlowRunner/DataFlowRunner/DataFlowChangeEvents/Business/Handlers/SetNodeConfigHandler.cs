using DataFlowRunner.DataFlowChangeEvents.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowChangeEvents.Business.Handlers;

internal sealed class SetNodeConfigHandler : IDataFlowChangeEventHandler<SetNodeConfig>
{
	private readonly IDataFlowInvalidator _dataFlowInvalidator;

	public SetNodeConfigHandler(IDataFlowInvalidator dataFlowInvalidator)
	{
		_dataFlowInvalidator = dataFlowInvalidator ?? throw new ArgumentNullException(nameof(dataFlowInvalidator));
	}

	public Task HandleAsync(DataFlowChangeContext context, SetNodeConfig @event, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(@event);

		InvalidateNodeAndDownstream(@event, context, cancellationToken);
		return Task.CompletedTask;
	}

	private void InvalidateNodeAndDownstream(SetNodeConfig @event,
		DataFlowChangeContext context,
		CancellationToken cancellationToken)
	{
		var nodeId = new NodeId(@event.Id);
		if (!context.Graph.NodeExists(nodeId)) return;

		_dataFlowInvalidator.InvalidateNodeAndDownstreamAsync(
			context.ToRunningExecutionContext(), nodeId, cancellationToken);
	}
}
