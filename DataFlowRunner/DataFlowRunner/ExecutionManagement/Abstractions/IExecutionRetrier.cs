using DataFlowRunner.ExecutionManagement.Models;

namespace DataFlowRunner.ExecutionManagement.Abstractions;

public interface IExecutionRetrier
{
	Task RetryExecutionAsync(RetryExecutionCommandDto commandDto, CancellationToken cancellationToken);
}
