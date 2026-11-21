using DataFlowRunner.DataFlowAuthorization.Exceptions;
using DataFlowRunner.DataFlowExecution.Models;

namespace DataFlowRunner.DataFlowAuthorization.Abstractions;

/// <summary>
/// Validates user access to data flow executions.
/// </summary>
internal interface IDataFlowExecutionUserAuthorizer
{
	/// <summary>
	/// Validates that the current user has access to the specified data flow execution.
	/// </summary>
	/// <param name="dataFlowExecutionId">The data flow execution identifier to validate.</param>
	/// <param name="cancellationToken">Token used to cancel the validation request.</param>
	/// <exception cref="DataFlowExecutionUnauthorizedException">
	/// Thrown when the current user is not authorized for the specified execution.
	/// </exception>
	/// <exception cref="DataFlowExecutionAuthorizationFailedException">
	/// Thrown when authorization fails due to unexpected upstream errors.
	/// </exception>
	Task AuthorizeUserAccessAsync(
		DataFlowExecutionId dataFlowExecutionId,
		CancellationToken cancellationToken);
}
