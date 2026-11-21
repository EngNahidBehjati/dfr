using DataFlowRunner.Common.Authentication.Abstractions;
using DataFlowRunner.DataFlowSnapshot.Abstractions;
using DataFlowRunner.DataFlowSnapshot.Models;
using DataFlowRunner.NodePortConnections.Models;

namespace DataFlowRunner.DataFlowSnapshot.Business;

internal sealed class EdgeByPortSnapshotFetcher : IEdgeByPortSnapshotFetcher
{
	private readonly IDataFlowTopologyClientApi _dataFlowTopologyClientApi;
	private readonly IAuthenticationTokenAccessor _authenticationTokenAccessor;
	private readonly IEdgeByPortMapper _mapper;

	public EdgeByPortSnapshotFetcher(IDataFlowTopologyClientApi dataFlowTopologyClientApi,
		IAuthenticationTokenAccessor authenticationTokenAccessor,
		IEdgeByPortMapper mapper)
	{
		_dataFlowTopologyClientApi = dataFlowTopologyClientApi ?? throw new ArgumentNullException(nameof(dataFlowTopologyClientApi));
		_authenticationTokenAccessor = authenticationTokenAccessor ?? throw new ArgumentNullException(nameof(authenticationTokenAccessor));
		_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
	}

	public async Task<IReadOnlyList<EdgeByPort>> GetEdgeByPortAsync(Guid requestDataFlowId,
		CancellationToken cancellationToken, long? version = null)
	{
		DataFlowTopologyDto dataFlowTopology = await _dataFlowTopologyClientApi.GetDataFlowTopologyAsync(requestDataFlowId,
				_authenticationTokenAccessor.GetAuthenticationCookie(),
				version,
				cancellationToken)
			.ConfigureAwait(false);

		return _mapper.MapToEdgeByPorts(dataFlowTopology.Topology.Edges);
	}
}
