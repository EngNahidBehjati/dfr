using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Abstractions;

internal interface INodeExecutionTimingStore
{
	NodeExecutionTiming GetNodeExecutionTime(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId);

	void SetNodeExecutionStartTime(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId);

	void SetNodeExecutionEndTime(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId);
}
