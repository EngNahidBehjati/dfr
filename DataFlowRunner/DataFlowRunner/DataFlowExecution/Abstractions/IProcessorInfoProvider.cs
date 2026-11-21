using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;

namespace DataFlowRunner.DataFlowExecution.Abstractions;

internal interface IProcessorInfoProvider
{
	Task<ProcessorInfo> GetProcessorInfoAsync(DataFlowExecutionId dataFlowExecutionId, CancellationToken cancellationToken = default);
}
