using DataFlowRunner.ExecutionManagement.Models;

namespace DataFlowRunner.ExecutionManagement.Abstractions;

public interface IExecutionCanceller
{
	Task CancelExecutionAsync(CancelCommandDto commandDto,
		CancellationToken cancellationToken);
}
