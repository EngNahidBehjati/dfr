using DataFlowRunner.Common.Authentication.Exceptions;
using DataFlowRunner.Common.Authentication.Models;

namespace DataFlowRunner.Common.Authentication.Abstractions;

/// <summary>
/// Provides access to the current authentication token derived from the active HTTP context.
/// </summary>
internal interface IAuthenticationTokenAccessor
{
	/// <summary>
	/// Retrieves an authentication cookie value for the current user.
	/// </summary>
	/// <returns>Cookie formatted as <c>{auth token header name}={token}</c>, using the header name from AccessProvider.</returns>
	/// <exception cref="AuthenticationTokenNotFoundException">Thrown when no token is available in the HTTP context.</exception>
	AuthenticationCookie GetAuthenticationCookie();
}
