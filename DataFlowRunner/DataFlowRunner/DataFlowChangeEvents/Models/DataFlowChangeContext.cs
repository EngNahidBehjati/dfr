using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.GraphCore.Abstractions;

namespace DataFlowRunner.DataFlowChangeEvents.Models;

internal sealed record DataFlowChangeContext(
	DataFlowExecutionId DataFlowExecutionId,
	DataFlowVersion RunningVersion,
	DataFlowVersion NewVersion,
	IGraph Graph,
	ProcessorInfo ProcessorInfo,
	ExecutionSettings ExecutionSettings)
{
	public ProcessorInfo ProcessorInfo { get; set; } = ProcessorInfo;
	public ExecutionSettings ExecutionSettings { get; set; } = ExecutionSettings;

	public DataFlowExecutionContext ToRunningExecutionContext() =>
		new()
		{
			Graph = Graph,
			DataFlowExecutionId = DataFlowExecutionId,
			DataFlowVersion = RunningVersion,
			ProcessorInfo = ProcessorInfo,
			ExecutionSettings = ExecutionSettings
		};
}
