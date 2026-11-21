using DataFlowRunner.DataFlowSnapshot.Models;
using DataFlowRunner.NodePortConnections.Models;

namespace DataFlowRunner.DataFlowSnapshot.Abstractions;

internal interface IEdgeByPortMapper
{
	IReadOnlyList<EdgeByPort> MapToEdgeByPorts(IEnumerable<EdgeDto> edges);
}
