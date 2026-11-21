namespace DataFlowRunner.DataFlowAuthorization.Exceptions;

public sealed class DataFlowExecutionNotFoundOrAccessDeniedException : Exception
{
	public DataFlowExecutionNotFoundOrAccessDeniedException()
	{
	}

	public DataFlowExecutionNotFoundOrAccessDeniedException(string message) : base(message)
	{
	}

	public DataFlowExecutionNotFoundOrAccessDeniedException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
