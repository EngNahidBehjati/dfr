namespace DataFlowRunner.Common.Authentication.Exceptions;

/// <summary>
/// Exception thrown when the authentication token cannot be found in the HTTP context.
/// </summary>
public sealed class AuthenticationTokenNotFoundException : Exception
{
	public AuthenticationTokenNotFoundException()
	{
	}

	public AuthenticationTokenNotFoundException(string message) : base(message)
	{
	}

	public AuthenticationTokenNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
