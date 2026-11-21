namespace DataFlowRunner.DataFlowAuthorization.Exceptions;

public sealed class DataFlowExecutionUnauthorizedException : Exception
{
	public DataFlowExecutionUnauthorizedException()
	{
	}

	public DataFlowExecutionUnauthorizedException(string message) : base(message)
	{
	}

	public DataFlowExecutionUnauthorizedException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
