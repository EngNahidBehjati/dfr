namespace DataFlowRunner.GraphCore.Exceptions.Conflict;

public sealed class DuplicateEdgeException : Exception
{
	public DuplicateEdgeException()
	{
	}

	public DuplicateEdgeException(Guid edgeId)
		: base($"An edge with ID '{edgeId}' already exists in the graph.")
	{
	}

	public DuplicateEdgeException(string? message) : base(message)
	{
	}

	public DuplicateEdgeException(string? message, Exception? innerException) : base(message, innerException)
	{
	}
}
