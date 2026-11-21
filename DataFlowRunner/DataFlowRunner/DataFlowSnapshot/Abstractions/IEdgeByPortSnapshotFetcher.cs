using DataFlowRunner.NodePortConnections.Models;

namespace DataFlowRunner.DataFlowSnapshot.Abstractions;

internal interface IEdgeByPortSnapshotFetcher
{
	Task<IReadOnlyList<EdgeByPort>> GetEdgeByPortAsync(Guid requestDataFlowId, CancellationToken cancellationToken,
		long? version = null);
}
