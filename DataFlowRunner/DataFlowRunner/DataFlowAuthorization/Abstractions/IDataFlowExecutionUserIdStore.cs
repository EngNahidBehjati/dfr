using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowExecution.Models;

namespace DataFlowRunner.DataFlowAuthorization.Abstractions;

/// <summary>
/// Stores and retrieves user identifiers associated with data flow executions.
/// </summary>
internal interface IDataFlowExecutionUserIdStore : IDataFlowExecutionUserIdProvider
{
	/// <summary>
	/// Associates a user ID with the specified execution identifier.
	/// </summary>
	/// <param name="dataFlowExecutionId">Execution identifier to map.</param>
	/// <param name="userId">User ID to store.</param>
	void StoreDataFlowExecutionUserId(DataFlowExecutionId dataFlowExecutionId, UserId userId);
}
