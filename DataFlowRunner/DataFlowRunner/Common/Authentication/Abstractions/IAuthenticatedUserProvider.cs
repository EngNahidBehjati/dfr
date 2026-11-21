using DataFlowRunner.Common.Authentication.Exceptions;
using DataFlowRunner.Common.Authentication.Models;

namespace DataFlowRunner.Common.Authentication.Abstractions;

/// <summary>
/// Retrieves the ID of the currently authenticated user from the authentication provider.
/// </summary>
internal interface IAuthenticatedUserProvider
{
	/// <summary>
	/// Gets the user ID of the currently authenticated user.
	/// </summary>
	/// <param name="cancellationToken">Token used to cancel the retrieval request.</param>
	/// <returns>User ID of the currently authenticated user.</returns>
	/// <exception cref="AuthenticatedUserRetrievalException">Thrown when the user ID claim is missing or invalid.</exception>
	Task<UserId> GetCurrentUserId(CancellationToken cancellationToken);
}
