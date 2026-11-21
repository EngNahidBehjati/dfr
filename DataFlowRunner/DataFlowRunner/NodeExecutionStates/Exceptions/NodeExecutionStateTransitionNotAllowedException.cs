namespace DataFlowRunner.NodeExecutionStates.Exceptions;

public class NodeExecutionStateTransitionNotAllowedException : Exception
{
	public NodeExecutionStateTransitionNotAllowedException()
	{
	}

	public NodeExecutionStateTransitionNotAllowedException(string? message) : base(message)
	{
	}

	public NodeExecutionStateTransitionNotAllowedException(string? message, Exception? innerException) : base(message, innerException)
	{
	}
}
