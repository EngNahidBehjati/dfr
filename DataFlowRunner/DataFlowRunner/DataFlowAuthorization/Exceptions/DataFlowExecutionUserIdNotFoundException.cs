namespace DataFlowRunner.DataFlowAuthorization.Exceptions;

/// <summary>
/// Exception thrown when a user ID is not found for the specified data flow execution.
/// </summary>
public sealed class DataFlowExecutionUserIdNotFoundException : Exception
{
	public DataFlowExecutionUserIdNotFoundException()
	{
	}

	public DataFlowExecutionUserIdNotFoundException(string message) : base(message)
	{
	}

	public DataFlowExecutionUserIdNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
