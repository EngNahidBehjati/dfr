namespace DataFlowRunner.ExecutionManagement.Exceptions;

public sealed class RetryExecutionFailedException : Exception
{
	public RetryExecutionFailedException()
	{
	}

	public RetryExecutionFailedException(string? message) : base(message)
	{
	}

	public RetryExecutionFailedException(string? message, Exception? innerException)
		: base(message, innerException)
	{
	}
}
