using DataFlowRunner.Common.Exceptions.Abstraction;
using DataFlowRunner.Common.Exceptions.Constants;

namespace DataFlowRunner.Common.Exceptions.Business;

internal sealed class StackTraceResolver : IStackTraceResolver
{
	public string ResolveStackTrace(Exception exception)
	{
		var stackTrace = exception.StackTrace ?? string.Empty;
		if (exception.Data.Contains(StackTraceConstants.NodeExecutorEngine))
		{
			return stackTrace + "\nError in NodeExecutorEngine:\n" +
			       exception.Data[StackTraceConstants.NodeExecutorEngine];
		}
		if (exception.Data.Contains(StackTraceConstants.DataFlowBuilder))
		{
			return stackTrace + "\nError in DataFlowBuilder:\n" +
			       exception.Data[StackTraceConstants.DataFlowBuilder];
		}

		return stackTrace;
	}
}
