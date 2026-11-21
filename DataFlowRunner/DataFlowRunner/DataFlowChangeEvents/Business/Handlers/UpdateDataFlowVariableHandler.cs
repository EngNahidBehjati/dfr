using DataFlowRunner.DataFlowChangeEvents.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;

namespace DataFlowRunner.DataFlowChangeEvents.Business.Handlers;

internal sealed class UpdateDataFlowVariableHandler : IDataFlowChangeEventHandler<UpdateDataFlowVariable>
{
	public Task HandleAsync(DataFlowChangeContext context, UpdateDataFlowVariable @event, CancellationToken cancellationToken) =>
		Task.CompletedTask;
}
