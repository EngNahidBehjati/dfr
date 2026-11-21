using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowNodeConfigs.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowNodeConfigs.Abstractions;

internal interface IDataFlowNodeConfigsStore
{
	NodeConfig GetNodeConfig(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId);
	void SetNodeConfig(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId, NodeConfig nodeConfig);
}
