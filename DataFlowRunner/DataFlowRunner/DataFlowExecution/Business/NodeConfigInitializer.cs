using DataFlowRunner.Common.Authentication.Abstractions;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowNodeConfigs.Abstractions;
using DataFlowRunner.DataFlowNodeConfigs.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class NodeConfigInitializer : INodeConfigInitializer
{
	private readonly IDataFlowNodeConfigsStore _dataFlowNodeConfigsStore;
	private readonly INodeConfigRetrieveClientApi _nodeConfigRetrieveClientApi;
	private readonly IAuthenticationTokenAccessor _authenticationTokenAccessor;

	public NodeConfigInitializer(IDataFlowNodeConfigsStore dataFlowNodeConfigsStore,
		INodeConfigRetrieveClientApi nodeConfigRetrieveClientApi,
		IAuthenticationTokenAccessor authenticationTokenAccessor)
	{
		_dataFlowNodeConfigsStore = dataFlowNodeConfigsStore ??
										throw new ArgumentNullException(nameof(dataFlowNodeConfigsStore));
		_nodeConfigRetrieveClientApi = nodeConfigRetrieveClientApi ??
									   throw new ArgumentNullException(nameof(nodeConfigRetrieveClientApi));
		_authenticationTokenAccessor = authenticationTokenAccessor ??
									   throw new ArgumentNullException(nameof(authenticationTokenAccessor));
	}

	public async Task EnsureNodeConfigsLoadedAsync(DataFlowExecutionId dataFlowExecutionId,
		IReadOnlyCollection<Node> nodeIds, CancellationToken cancellationToken)
	{
		List<RequestNodeConfigDto> requestNodeConfigs = GetNodeVersions(nodeIds);
		ResponseDataFlowNodeConfigsDto responseDataFlowNodeConfigs =
			await RetrieveDataFlowNodeConfigsAsync(dataFlowExecutionId, requestNodeConfigs, cancellationToken)
				.ConfigureAwait(false);

		foreach (ResponseNodeConfigDto responseNodeConfig in responseDataFlowNodeConfigs.ResponseNodeConfigs)
		{
			_dataFlowNodeConfigsStore.SetNodeConfig(dataFlowExecutionId, new NodeId(responseNodeConfig.NodeId),
				new NodeConfig { Version = new NodeVersion(responseNodeConfig.Version), Content = responseNodeConfig.Config });
		}
	}

	private async Task<ResponseDataFlowNodeConfigsDto> RetrieveDataFlowNodeConfigsAsync(DataFlowExecutionId dataFlowExecutionId,
		List<RequestNodeConfigDto> requestNodeConfigs, CancellationToken cancellationToken)
	{
		return await _nodeConfigRetrieveClientApi
			.RetrieveNodeConfigsAsync(dataFlowExecutionId.Value,
				_authenticationTokenAccessor.GetAuthenticationCookie(),
				new RequestDateFlowNodeConfigsDto(requestNodeConfigs),
				cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	private static List<RequestNodeConfigDto> GetNodeVersions(IReadOnlyCollection<Node> nodeIds)
	{
		return nodeIds.Select(n => new RequestNodeConfigDto(n.Id.Value, null)).ToList();
	}
}
