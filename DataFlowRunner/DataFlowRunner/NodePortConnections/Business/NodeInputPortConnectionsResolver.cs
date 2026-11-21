using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodePortConnections.Abstract;
using DataFlowRunner.NodePortConnections.Models;

namespace DataFlowRunner.NodePortConnections.Business;

internal sealed class NodeInputPortToUpstreamPortsResolver : INodeInputPortToUpstreamPortsResolver
{
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly IEdgeByPortContainer _edgeByPortContainer;

	public NodeInputPortToUpstreamPortsResolver(IDataFlowExecutionContextProvider dataFlowExecutionContextProvider, IEdgeByPortContainer edgeByPortContainer)
	{
		_dataFlowExecutionContextProvider = dataFlowExecutionContextProvider ?? throw new ArgumentNullException(nameof(dataFlowExecutionContextProvider));
		_edgeByPortContainer = edgeByPortContainer ?? throw new ArgumentNullException(nameof(edgeByPortContainer));
	}

	public async Task<IReadOnlyCollection<NodeInputPortToUpstreamPorts>> GetNodeInputPortsToUpstreamPortsAsync(
		DataFlowExecutionId dataFlowExecutionId,
		NodeId nodeId, CancellationToken cancellationToken)
	{
		DataFlowExecutionContext dataFlowExecutionContext = await _dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(dataFlowExecutionId, cancellationToken).ConfigureAwait(false);
		IReadOnlyCollection<EdgeByNode> edgeByNodes = dataFlowExecutionContext.Graph.GetAllEdges();
		IReadOnlyCollection<EdgeByPort> edgeByPorts = GetNodeInputEdgeByPorts(nodeId, edgeByNodes);
		return GetNodeInputPortsToUpstreamPorts(edgeByPorts);
	}

	private static List<NodeInputPortToUpstreamPorts> GetNodeInputPortsToUpstreamPorts(IReadOnlyCollection<EdgeByPort> edgeByPorts)
	{
		var result = edgeByPorts
			.GroupBy(e => e.ToPortId.Value)
			.Select(g => new NodeInputPortToUpstreamPorts
			{
				InputPort = g.Key,
				UpstreamPorts = g.Select(e => e.FromPortId.Value).ToList()
			})
			.ToList();
		return result;
	}

	private List<EdgeByPort> GetNodeInputEdgeByPorts(NodeId nodeId, IReadOnlyCollection<EdgeByNode> edgeByNodes)
	{
		return edgeByNodes
			.Where(e => e.ToNodeId == nodeId)
			.Select(e => _edgeByPortContainer.GetEdgeByPort(e.Id)).ToList();
	}
}
