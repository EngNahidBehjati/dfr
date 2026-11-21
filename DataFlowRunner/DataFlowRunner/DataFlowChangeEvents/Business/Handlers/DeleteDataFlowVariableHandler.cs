using DataFlowRunner.DataFlowChangeEvents.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;

namespace DataFlowRunner.DataFlowChangeEvents.Business.Handlers;

internal sealed class DeleteDataFlowVariableHandler : IDataFlowChangeEventHandler<DeleteDataFlowVariable>
{
	public Task HandleAsync(DataFlowChangeContext context, DeleteDataFlowVariable @event, CancellationToken cancellationToken) =>
		Task.CompletedTask;
}
