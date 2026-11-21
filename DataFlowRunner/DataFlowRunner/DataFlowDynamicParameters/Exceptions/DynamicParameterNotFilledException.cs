namespace DataFlowRunner.DataFlowDynamicParameters.Exceptions;

public sealed class DynamicParameterNotFilledException : Exception
{
	public DynamicParameterNotFilledException()
	{
	}

	public DynamicParameterNotFilledException(string? message) : base(message)
	{
	}

	public DynamicParameterNotFilledException(string? message, Exception? innerException) :
		base(message, innerException)
	{
	}
}
