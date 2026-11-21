namespace DataFlowRunner.Common.Authentication.Exceptions;

/// <summary>
/// Exception thrown when current user information cannot be retrieved.
/// </summary>
public sealed class AuthenticatedUserRetrievalException : Exception
{
	public AuthenticatedUserRetrievalException()
	{
	}

	public AuthenticatedUserRetrievalException(string message) : base(message)
	{
	}

	public AuthenticatedUserRetrievalException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
