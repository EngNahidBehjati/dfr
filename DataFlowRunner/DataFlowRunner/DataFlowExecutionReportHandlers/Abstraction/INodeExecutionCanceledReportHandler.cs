using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;

namespace DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;

public interface INodeExecutionCanceledReportHandler
{
	Task HandleAsync(NodeExecutionCanceledReport report, CancellationToken cancellationToken);
}
