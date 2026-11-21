using DataFlowRunner.ExecutionManagement.Models;

namespace DataFlowRunner.ExecutionManagement.Abstractions;

internal interface IRetryExecutionCommandInvoker
{
	Task<RetryExecutionReport> InvokeAsync(RetryExecutionCommand command, CancellationToken cancellationToken);
}
