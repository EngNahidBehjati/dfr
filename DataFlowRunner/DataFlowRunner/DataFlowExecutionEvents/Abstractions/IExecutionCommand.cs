namespace DataFlowRunner.DataFlowExecutionEvents.Abstractions;

internal interface IExecutionCommand
{
	Guid GraphExecutionId { get; }
	Guid UserId { get; }
	IReadOnlyDictionary<string, object> GetLogParameters();
}
