using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.TableQuery.Abstractions;

internal interface INodeExecutionStateValidator
{
	void ValidateNodeIsSucceeded(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId);
}
