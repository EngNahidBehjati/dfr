using DataFlowRunner.DataFlowSnapshot.Models;

namespace DataFlowRunner.Integration.Tests.DataFlowExecution.Helpers;

internal sealed class TopologyDtoCreator
{
	/// <summary>
	/// Creates a topology DTO representing a data flow graph structure.
	/// The topology follows this structure:
	/// <code>
	///         (1) Root
	///         |        \
	///         |         \
	///         v          v
	///    (2) Node A      (4) Node B
	///         |     \           |
	///         |        \        |
	///         v           v     v
	/// (3) Node A Child ---> (5) Join
	/// </code>
	/// The Root node (1) splits into two parallel paths: Node A (2) and Node B (4).
	/// Node A (2) has two outputs: one to Node A Child (3) and another directly to Join (5).
	/// Node B (4) connects directly to Join (5).
	/// Node A Child (3) also connects to Join (5), where all paths converge.
	/// </summary>
	/// <returns>A <see cref="TopologyDto"/> representing the topology structure.</returns>
	public TopologyDto Create() =>
		new(BuildNodes(), BuildEdges(),
			new ProcessorInfoDto("PROCESSOR_LIST", Guid.NewGuid()),
			new ExecutionSettingsDto("PIPELINE_EXECUTOR", null));

	private List<NodeDto> BuildNodes() =>
	[
		new(RootNodeId,
			"Default",
			[
				new PortDto(RootToNodeAPortId, PortDirectionDto.Out),
				new PortDto(RootToNodeBPortId, PortDirectionDto.Out)
			],
			Version: 99),
		new(NodeANodeId,
			"Default",
			[
				new PortDto(NodeAInputPortId, PortDirectionDto.In),
				new PortDto(NodeAToChildPortId, PortDirectionDto.Out),
				new PortDto(NodeAToJoinPortId, PortDirectionDto.Out)
			],
			Version: 100),
		new(NodeAChildNodeId,
			"Default",
			[
				new PortDto(NodeAChildInputPortId, PortDirectionDto.In),
				new PortDto(NodeAChildToJoinPortId, PortDirectionDto.Out)
			],
			Version: 101),
		new(NodeBNodeId,
			"Default",
			[
				new PortDto(NodeBInputPortId, PortDirectionDto.In),
				new PortDto(NodeBToJoinPortId, PortDirectionDto.Out)
			],
			Version: 102),
		new(JoinNodeId,
			"Default",
			[
				new PortDto(JoinFromNodeAPortId, PortDirectionDto.In),
				new PortDto(JoinFromNodeBPortId, PortDirectionDto.In),
				new PortDto(JoinFromNodeAChildPortId, PortDirectionDto.In)
			],
			Version: 103)
	];

	private List<EdgeDto> BuildEdges() =>
	[
		new(RootToNodeAEdgeId, RootToNodeAPortId, NodeAInputPortId, RootNodeId, NodeANodeId),
		new(RootToNodeBEdgeId, RootToNodeBPortId, NodeBInputPortId, RootNodeId, NodeBNodeId),
		new(NodeAToChildEdgeId, NodeAToChildPortId, NodeAChildInputPortId, NodeANodeId, NodeAChildNodeId),
		new(NodeAToJoinEdgeId, NodeAToJoinPortId, JoinFromNodeAPortId, NodeANodeId, JoinNodeId),
		new(NodeBToJoinEdgeId, NodeBToJoinPortId, JoinFromNodeBPortId, NodeBNodeId, JoinNodeId),
		new(NodeAChildToJoinEdgeId, NodeAChildToJoinPortId, JoinFromNodeAChildPortId, NodeAChildNodeId, JoinNodeId)
	];

	public readonly Guid RootNodeId = Guid.NewGuid();
	public readonly Guid NodeANodeId = Guid.NewGuid();
	public readonly Guid NodeAChildNodeId = Guid.NewGuid();
	public readonly Guid NodeBNodeId = Guid.NewGuid();
	public readonly Guid JoinNodeId = Guid.NewGuid();

	public readonly Guid RootToNodeAPortId = Guid.NewGuid();
	public readonly Guid RootToNodeBPortId = Guid.NewGuid();
	public readonly Guid NodeAInputPortId = Guid.NewGuid();
	public readonly Guid NodeAToChildPortId = Guid.NewGuid();
	public readonly Guid NodeAToJoinPortId = Guid.NewGuid();
	public readonly Guid NodeAChildInputPortId = Guid.NewGuid();
	public readonly Guid NodeAChildToJoinPortId = Guid.NewGuid();
	public readonly Guid NodeBInputPortId = Guid.NewGuid();
	public readonly Guid NodeBToJoinPortId = Guid.NewGuid();
	public readonly Guid JoinFromNodeAPortId = Guid.NewGuid();
	public readonly Guid JoinFromNodeBPortId = Guid.NewGuid();
	public readonly Guid JoinFromNodeAChildPortId = Guid.NewGuid();

	public readonly Guid RootToNodeAEdgeId = Guid.NewGuid();
	public readonly Guid RootToNodeBEdgeId = Guid.NewGuid();
	public readonly Guid NodeAToChildEdgeId = Guid.NewGuid();
	public readonly Guid NodeAToJoinEdgeId = Guid.NewGuid();
	public readonly Guid NodeBToJoinEdgeId = Guid.NewGuid();
	public readonly Guid NodeAChildToJoinEdgeId = Guid.NewGuid();
}
