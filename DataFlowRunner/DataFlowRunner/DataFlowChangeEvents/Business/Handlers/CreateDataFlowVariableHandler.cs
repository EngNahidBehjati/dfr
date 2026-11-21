using DataFlowRunner.DataFlowChangeEvents.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;

namespace DataFlowRunner.DataFlowChangeEvents.Business.Handlers;

internal sealed class CreateDataFlowVariableHandler : IDataFlowChangeEventHandler<CreateDataFlowVariable>
{
	public Task HandleAsync(DataFlowChangeContext context, CreateDataFlowVariable @event, CancellationToken cancellationToken) =>
		Task.CompletedTask;
}
