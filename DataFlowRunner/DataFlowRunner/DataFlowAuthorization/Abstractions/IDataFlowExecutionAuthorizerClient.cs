using DataFlowRunner.DataFlowExecution.Models;

namespace DataFlowRunner.DataFlowAuthorization.Abstractions;

internal interface IDataFlowExecutionAuthorizerClient
{
	Task AuthorizeDataFlowExecutionAsync(
		DataFlowExecutionId dataFlowExecutionId,
		CancellationToken cancellationToken);
}
