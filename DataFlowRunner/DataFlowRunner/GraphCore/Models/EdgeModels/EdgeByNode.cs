using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.GraphCore.Models.EdgeModels;

internal sealed class EdgeByNode
{
	public required EdgeId Id { get; init; }
	public required NodeId FromNodeId { get; init; }
	public required NodeId ToNodeId { get; init; }
}
