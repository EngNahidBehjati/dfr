namespace DataFlowRunner.DataFlowAuthorization.Exceptions;

public sealed class DataFlowExecutionIdNotFoundInRouteException : Exception
{
	public DataFlowExecutionIdNotFoundInRouteException()
	{
	}

	public DataFlowExecutionIdNotFoundInRouteException(string message) : base(message)
	{
	}

	public DataFlowExecutionIdNotFoundInRouteException(string message, Exception innerException) :
		base(message, innerException)
	{
	}
}
