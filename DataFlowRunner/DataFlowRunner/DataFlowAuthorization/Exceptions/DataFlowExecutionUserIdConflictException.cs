namespace DataFlowRunner.DataFlowAuthorization.Exceptions;

/// <summary>
/// Exception thrown when attempting to set a different user ID for an existing data flow execution.
/// </summary>
public sealed class DataFlowExecutionUserIdConflictException : Exception
{
	public DataFlowExecutionUserIdConflictException()
	{
	}

	public DataFlowExecutionUserIdConflictException(string message) : base(message)
	{
	}

	public DataFlowExecutionUserIdConflictException(string message, Exception innerException) :
		base(message, innerException)
	{
	}
}
