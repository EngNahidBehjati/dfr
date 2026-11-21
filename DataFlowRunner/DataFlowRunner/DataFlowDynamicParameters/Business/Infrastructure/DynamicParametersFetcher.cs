using DataFlowRunner.Common.Authentication.Abstractions;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Infrastructure;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowDynamicParameters.Business.Infrastructure;

internal sealed class DynamicParametersFetcher : IDynamicParametersFetcher
{
	private readonly IAuthenticationTokenAccessor _authenticationTokenAccessor;
	private readonly IDataFlowDynamicParametersClientApi _dataFlowDynamicParametersClientApi;

	public DynamicParametersFetcher(IAuthenticationTokenAccessor authenticationTokenAccessor,
		IDataFlowDynamicParametersClientApi dataFlowDynamicParametersClientApi)
	{
		_authenticationTokenAccessor = authenticationTokenAccessor ??
		                               throw new ArgumentNullException(nameof(authenticationTokenAccessor));
		_dataFlowDynamicParametersClientApi = dataFlowDynamicParametersClientApi ??
		                                      throw new ArgumentNullException(nameof(dataFlowDynamicParametersClientApi));
	}

	public async Task<ResponseDataFlowDynamicParametersDto> FetchDataFlowDynamicParametersAsync(
		DataFlowExecutionId dataFlowExecutionId,
		List<NodeId> nodeIds,
		CancellationToken cancellationToken)
	{
		AuthenticationCookie authenticationCookie = _authenticationTokenAccessor.GetAuthenticationCookie();

		ResponseDataFlowDynamicParametersDto response = await _dataFlowDynamicParametersClientApi
			.GetDataFlowDynamicParametersAsync(
				dataFlowExecutionId.Value,
				authenticationCookie,
				new RequestDataFlowDynamicParametersDto(nodeIds.Select(n => n.Value).ToList()),
				cancellationToken).ConfigureAwait(false);

		return response;
	}
}
