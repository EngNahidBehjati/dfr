namespace DataFlowRunner.TableQuery.Exceptions;

public sealed class NodeExecutionStateNotSucceededException : Exception
{
	public NodeExecutionStateNotSucceededException(string message) : base(message)
	{
	}
	public NodeExecutionStateNotSucceededException()
	{
	}
}
