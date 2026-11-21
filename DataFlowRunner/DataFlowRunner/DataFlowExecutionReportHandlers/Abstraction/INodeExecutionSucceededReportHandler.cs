using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;

namespace DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;

public interface INodeExecutionSucceededReportHandler
{
	Task HandleAsync(NodeExecutionSucceededReport report, CancellationToken cancellationToken);
}
