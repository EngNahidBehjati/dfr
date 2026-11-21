using DataFlowRunner.DataFlowChangeEvents.Models;

namespace DataFlowRunner.DataFlowChangeEvents.Abstractions;

internal interface IDataFlowChangeEventHandler<in TEvent>
{
	Task HandleAsync(DataFlowChangeContext context, TEvent @event, CancellationToken cancellationToken);
}
