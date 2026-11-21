using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;

namespace DataFlowRunner.Tests.Common;

internal static class DefaultProcessorInfoCreator
{
	public static ProcessorInfo Create()
	{
		return new ProcessorInfo
		{
			ProcessorType = "SYSTEM_DEFAULT_PROCESSOR",
			ProcessorId = null
		};
	}
}
