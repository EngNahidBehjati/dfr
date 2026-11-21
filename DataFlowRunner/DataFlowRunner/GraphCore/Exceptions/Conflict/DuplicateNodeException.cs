namespace DataFlowRunner.GraphCore.Exceptions.Conflict;

public sealed class DuplicateNodeException : Exception
{
	public DuplicateNodeException()
	{
	}

	public DuplicateNodeException(string? message) : base(message)
	{
	}

	public DuplicateNodeException(string? message, Exception? innerException) : base(message, innerException)
	{
	}
}
