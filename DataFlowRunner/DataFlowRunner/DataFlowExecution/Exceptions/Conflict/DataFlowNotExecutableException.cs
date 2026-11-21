namespace DataFlowRunner.DataFlowExecution.Exceptions.Conflict;

public sealed class DataFlowNotExecutableException : Exception
{
	public DataFlowNotExecutableException()
	{
	}

	public DataFlowNotExecutableException(string? message) : base(message)
	{
	}

	public DataFlowNotExecutableException(string? message, Exception? innerException)
		: base(message, innerException)
	{
	}
}
