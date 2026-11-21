using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.PortModels;

namespace DataFlowRunner.NodePortConnections.Models;

internal class EdgeByPort
{
	public required EdgeId Id { get; init; }
	public required OutputPortId FromPortId { get; init; }
	public required InputPortId ToPortId { get; init; }
}
