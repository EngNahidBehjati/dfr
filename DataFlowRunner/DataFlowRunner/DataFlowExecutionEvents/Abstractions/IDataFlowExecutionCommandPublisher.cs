namespace DataFlowRunner.DataFlowExecutionEvents.Abstractions;

internal interface IDataFlowExecutionCommandPublisher
{
	ValueTask PublishCommandAsync(IExecutionCommand command, CancellationToken cancellationToken);
}
