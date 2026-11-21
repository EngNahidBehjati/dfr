namespace DataFlowRunner.DataFlowExecution.Exceptions.Conflict;

public sealed class DataFlowVersionConflictException : Exception
{
	public DataFlowVersionConflictException()
	{
	}

	public DataFlowVersionConflictException(string? message) : base(message)
	{
	}

	public DataFlowVersionConflictException(string? message, Exception? innerException)
		: base(message, innerException)
	{
	}
}
