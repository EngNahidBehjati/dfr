namespace DataFlowRunner.NodeExecutionStates.Exceptions;

public sealed class NodeErrorMessageNotFoundException : Exception
{
	public NodeErrorMessageNotFoundException()
	{
	}

	public NodeErrorMessageNotFoundException(string? message) : base(message)
	{
	}

	public NodeErrorMessageNotFoundException(string? message, Exception? innerException)
		: base(message, innerException)
	{
	}
}
