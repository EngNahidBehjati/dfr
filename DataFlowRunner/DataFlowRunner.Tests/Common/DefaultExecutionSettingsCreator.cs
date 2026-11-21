using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;

namespace DataFlowRunner.Tests.Common;

internal static class DefaultExecutionSettingsCreator
{
	public static ExecutionSettings Create()
	{
		return new ExecutionSettings
		{
			ExecutorType = "PIPELINE_EXECUTOR",
			ExecutorId = null
		};
	}
}
