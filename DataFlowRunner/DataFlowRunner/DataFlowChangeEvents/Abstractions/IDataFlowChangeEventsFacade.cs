using DataFlowRunner.DataFlowChangeEvents.Events;
using Wolverine;

namespace DataFlowRunner.DataFlowChangeEvents.Abstractions;

public interface IDataFlowChangeEventsFacade
{
	Task HandleAsync(ChangeEvent changeEvent, Envelope envelope, CancellationToken cancellationToken);
}
