using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Abstractions;
using Wolverine.Tracking;

namespace DataFlowRunner.Integration.Tests.DataFlowExecution.Models;

internal sealed record DataFlowExecutionResult(
	HttpResponseContainer HttpResponseContainer,
	ITrackedSession TrackedSession)
{
	public IEnumerable<TCommand> GetPublishedCommands<TCommand>(Guid executionId)
		where TCommand : class, IWolverineExecutionCommand
	{
		return TrackedSession.Sent
			.AllMessages()
			.OfType<TCommand>()
			.Where(command => command.GraphExecutionId == executionId);
	}
}
