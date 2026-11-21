namespace DataFlowRunner.DataFlowAuthorization.Exceptions;

/// <summary>
/// Exception thrown when data flow execution validation fails due to server errors, connection issues, or other non-authorization failures.
/// </summary>
public sealed class DataFlowExecutionAuthorizationFailedException : Exception
{
	public DataFlowExecutionAuthorizationFailedException()
	{
	}

	public DataFlowExecutionAuthorizationFailedException(string message) : base(message)
	{
	}

	public DataFlowExecutionAuthorizationFailedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
