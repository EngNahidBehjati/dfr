namespace DataFlowRunner.Common.Exceptions.NotFound;

public sealed class DataFlowExecutionNotFoundException : Exception
{
	public DataFlowExecutionNotFoundException()
	{
	}
	public DataFlowExecutionNotFoundException(string? message) : base(message)
	{
	}

	public DataFlowExecutionNotFoundException(string? message, Exception? innerException)
		: base(message, innerException)
	{
	}
}
