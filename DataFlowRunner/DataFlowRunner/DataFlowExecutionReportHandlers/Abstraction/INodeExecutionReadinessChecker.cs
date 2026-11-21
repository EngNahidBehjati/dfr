using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;

internal interface INodeExecutionReadinessChecker
{
	bool IsNodeReadyToExecute(DataFlowExecutionContext dataFlowExecutionContext, NodeId nodeId);
}
