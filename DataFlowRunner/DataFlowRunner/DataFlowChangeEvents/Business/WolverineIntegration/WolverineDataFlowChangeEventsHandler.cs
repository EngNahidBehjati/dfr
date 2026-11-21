using DataFlowRunner.DataFlowChangeEvents.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Events;
using Wolverine;

namespace DataFlowRunner.DataFlowChangeEvents.Business.WolverineIntegration;

public sealed class WolverineDataFlowChangeEventsHandler
{
	private readonly IDataFlowChangeEventsFacade _facade;

	public WolverineDataFlowChangeEventsHandler(
		IDataFlowChangeEventsFacade facade)
	{
		_facade = facade ?? throw new ArgumentNullException(nameof(facade));
	}

	public async Task HandleAsync(ChangeEvent changeEvent, Envelope envelope, CancellationToken cancellationToken)
	{
		await _facade.HandleAsync(changeEvent, envelope, CancellationToken.None).ConfigureAwait(false);
	}
}
