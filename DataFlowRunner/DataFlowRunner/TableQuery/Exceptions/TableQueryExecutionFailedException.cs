using DataFlowRunner.Common.Exceptions.Constants;

namespace DataFlowRunner.TableQuery.Exceptions;

public sealed class TableQueryExecutionFailedException : Exception
{
	public TableQueryExecutionFailedException(string message, string remoteStackTrace) : base(message)
	{
		Data[StackTraceConstants.NodeExecutorEngine] = remoteStackTrace;
	}

	public TableQueryExecutionFailedException(string message) : base(message)
	{
	}
}
