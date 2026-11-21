using DataFlowRunner.DataFlowSnapshot.Abstractions;
using DataFlowRunner.DataFlowSnapshot.Models;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.PortModels;
using DataFlowRunner.NodePortConnections.Models;

namespace DataFlowRunner.DataFlowSnapshot.Mappers;

internal sealed class EdgeByPortMapper : IEdgeByPortMapper
{
	public IReadOnlyList<EdgeByPort> MapToEdgeByPorts(IEnumerable<EdgeDto> edges)
	{
		return edges.Select(dto => new EdgeByPort
		{
			Id = new EdgeId(dto.EdgeId),
			ToPortId = new InputPortId(dto.ToPort),
			FromPortId = new OutputPortId(dto.FromPort)
		}).ToList();
	}
}
