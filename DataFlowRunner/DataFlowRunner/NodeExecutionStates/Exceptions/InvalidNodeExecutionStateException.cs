namespace DataFlowRunner.NodeExecutionStates.Exceptions;

public sealed class InvalidNodeExecutionStateException : Exception
{
	public InvalidNodeExecutionStateException()
	{
	}

	public InvalidNodeExecutionStateException(string? message) : base(message)
	{
	}

	public InvalidNodeExecutionStateException(string? message, Exception? innerException) : base(message, innerException)
	{
	}
}
