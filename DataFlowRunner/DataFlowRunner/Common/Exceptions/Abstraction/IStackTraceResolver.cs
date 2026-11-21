namespace DataFlowRunner.Common.Exceptions.Abstraction;

public interface IStackTraceResolver
{
	string ResolveStackTrace(Exception exception);
}
