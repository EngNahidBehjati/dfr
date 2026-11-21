using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Exceptions;
using DataFlowRunner.DataFlowExecution.Models;

namespace DataFlowRunner.DataFlowAuthorization.Abstractions;

/// <summary>
/// Retrieves user identifiers associated with data flow executions.
/// </summary>
internal interface IDataFlowExecutionUserIdProvider
{
	/// <summary>
	/// Gets the user ID associated with the provided execution identifier.
	/// </summary>
	/// <param name="dataFlowExecutionId">Execution identifier used as the lookup key.</param>
	/// <returns>User ID mapped to the execution identifier.</returns>
	/// <exception cref="DataFlowExecutionUserIdNotFoundException">Thrown when no user ID is found for the provided identifier.</exception>
	UserId GetDataFlowExecutionUserId(DataFlowExecutionId dataFlowExecutionId);

	/// <summary>
	/// Checks and retrieve user ID if a user ID exists for the specified execution identifier.
	/// </summary>
	/// <param name="dataFlowExecutionId">Execution identifier to check.</param>
	/// <param name="userId">User ID mapped to the execution identifier.</param>
	/// <returns>True if a user ID exists for the execution identifier; otherwise, false.</returns>
	bool TryGetDataFlowExecutionUserId(DataFlowExecutionId dataFlowExecutionId,
		out UserId userId);
}
