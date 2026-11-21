using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Abstractions;

internal interface INodeExecutionErrorRecorder
{
	void RecordFailure(DataFlowExecutionId dataFlowExecutionId,
		NodeId nodeId,
		NodeErrorMessage nodeErrorMessage);

	void RecordCancellation(DataFlowExecutionId dataFlowExecutionId,
		NodeId nodeId,
		NodeErrorMessage nodeErrorMessage);
}
