using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Abstractions;

internal interface INodeExecutionTimeRecorder
{
	void RecordNodeExecutionTime(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId, NodeExecutionState lastState,
		NodeExecutionState nextState);
}
