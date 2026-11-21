using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using Refit;

namespace DataFlowRunner.DataFlowDynamicParameters.Abstractions.Infrastructure;

internal interface IDataFlowDynamicParametersClientApi
{
	[Post("/api/v1/pipelines/{pipelineId}/simple-variables")]
	[Headers("Accept: application/json")]
	Task<ResponseDataFlowDynamicParametersDto> GetDataFlowDynamicParametersAsync(
		[AliasAs("pipelineId")] Guid pipelineId,
		[Header("Cookie")] AuthenticationCookie authenticationCookie,
		[Body] RequestDataFlowDynamicParametersDto requestDynamicParametersDto,
		CancellationToken cancellationToken = default);
}
