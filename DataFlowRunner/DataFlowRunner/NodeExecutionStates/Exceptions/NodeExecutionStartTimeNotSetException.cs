namespace DataFlowRunner.NodeExecutionStates.Exceptions;

public sealed class NodeExecutionStartTimeNotSetException : Exception
{
	public NodeExecutionStartTimeNotSetException()
	{
	}

	public NodeExecutionStartTimeNotSetException(string? message) : base(message)
	{
	}

	public NodeExecutionStartTimeNotSetException(string? message, Exception? innerException)
		: base(message, innerException)
	{
	}
}
