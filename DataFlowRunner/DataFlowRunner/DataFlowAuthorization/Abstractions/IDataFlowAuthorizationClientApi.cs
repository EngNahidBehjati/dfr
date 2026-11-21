using DataFlowRunner.Common.Authentication.Models;
using Refit;

namespace DataFlowRunner.DataFlowAuthorization.Abstractions;

internal interface IDataFlowAuthorizationClientApi
{
	[Get("/api/v2/pipelines/{pipelineId}/topology")]
	[Headers("Accept: application/json")]
	Task CheckDataFlowAuthorizationAsync(
		[AliasAs("pipelineId")] Guid pipelineId,
		[Header("Cookie")] AuthenticationCookie authenticationCookie,
		[Query] long? version = null,
		CancellationToken cancellationToken = default);
}
