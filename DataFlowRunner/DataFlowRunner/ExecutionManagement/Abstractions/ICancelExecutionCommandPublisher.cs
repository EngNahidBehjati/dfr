using DataFlowRunner.ExecutionManagement.Models;

namespace DataFlowRunner.ExecutionManagement.Abstractions;

public interface ICancelExecutionCommandPublisher
{
	ValueTask PublishCommandAsync(CancelCommandDto commandDto,
		CancellationToken cancellationToken);
}
