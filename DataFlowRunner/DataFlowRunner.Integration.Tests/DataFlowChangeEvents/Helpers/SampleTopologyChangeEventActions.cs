using System.Text.Json;
using DataFlowRunner.DataFlowSnapshot.Models;
using DataFlowRunner.Integration.Tests.DataFlowExecution.Helpers;

namespace DataFlowRunner.Integration.Tests.DataFlowChangeEvents.Helpers;

internal sealed class SampleTopologyChangeEventActions
{
	private readonly TopologyDto _topologyDto = new TopologyDtoCreator().Create();

	public IReadOnlyList<Guid> NodeIds
		 => _topologyDto.Nodes.Select(node => node.NodeId).ToList();

	public IReadOnlyList<JsonElement> CreateAddPluginActions()
	{
		return NodeIds.Select(DataFlowChangeEventActionBuilder.AddPluginAction).ToList();
	}

	public IReadOnlyList<JsonElement> CreateAddLinkActions()
	{
		return _topologyDto.Edges
			.Select(edge => DataFlowChangeEventActionBuilder
				.AddLinkAction(edge.EdgeId, edge.FromPort, edge.ToPort, edge.FromNode, edge.ToNode))
			.ToList();
	}
}
