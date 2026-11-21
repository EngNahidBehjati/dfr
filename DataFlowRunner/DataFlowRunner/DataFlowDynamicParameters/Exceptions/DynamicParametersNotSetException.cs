namespace DataFlowRunner.DataFlowDynamicParameters.Exceptions;

public sealed class DynamicParametersNotSetException : Exception
{
	public DynamicParametersNotSetException()
	{
	}

	public DynamicParametersNotSetException(string? message) : base(message)
	{
	}

	public DynamicParametersNotSetException(string? message, Exception? innerException) :
		base(message, innerException)
	{
	}
}
