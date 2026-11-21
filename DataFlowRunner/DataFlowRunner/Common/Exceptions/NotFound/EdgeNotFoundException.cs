namespace DataFlowRunner.Common.Exceptions.NotFound;

public sealed class EdgeNotFoundException : Exception
{
	public EdgeNotFoundException()
	{
	}

	public EdgeNotFoundException(string? message) : base(message)
	{
	}

	public EdgeNotFoundException(string? message, Exception? innerException)
		: base(message, innerException)
	{
	}
}
