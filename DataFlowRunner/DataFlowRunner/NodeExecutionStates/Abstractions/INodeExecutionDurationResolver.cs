using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Abstractions;

internal interface INodeExecutionDurationResolver
{
	long? ResolveDurationMilliseconds(NodeExecutionTiming nodeExecutionTiming);
}
