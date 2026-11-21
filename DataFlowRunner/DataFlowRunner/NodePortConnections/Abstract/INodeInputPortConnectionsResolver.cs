using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.NodePortConnections.Abstract;

internal interface INodeInputPortToUpstreamPortsResolver
{
	Task<IReadOnlyCollection<NodeInputPortToUpstreamPorts>> GetNodeInputPortsToUpstreamPortsAsync(
		DataFlowExecutionId dataFlowExecutionId,
		NodeId nodeId, CancellationToken cancellationToken);
}
