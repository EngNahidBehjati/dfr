using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;

namespace DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;

public interface INodeExecutionFailedReportHandler
{
	Task HandleAsync(NodeExecutionFailedReport report, CancellationToken cancellationToken);
}
