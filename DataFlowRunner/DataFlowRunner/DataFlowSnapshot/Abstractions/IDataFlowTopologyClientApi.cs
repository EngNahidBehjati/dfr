using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowSnapshot.Models;
using Refit;

namespace DataFlowRunner.DataFlowSnapshot.Abstractions;

internal interface IDataFlowTopologyClientApi
{
	[Get("/api/v2/pipelines/{pipelineId}/topology")]
	[Headers("Accept: application/json")]
	Task<DataFlowTopologyDto> GetDataFlowTopologyAsync(
		[AliasAs("pipelineId")] Guid pipelineId,
		[Header("Cookie")] AuthenticationCookie authenticationCookie,
		[Query] long? version = null,
		CancellationToken cancellationToken = default);
}
