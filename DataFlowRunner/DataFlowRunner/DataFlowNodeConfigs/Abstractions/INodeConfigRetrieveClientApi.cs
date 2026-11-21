using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowNodeConfigs.Models;
using Refit;

namespace DataFlowRunner.DataFlowNodeConfigs.Abstractions;

public interface INodeConfigRetrieveClientApi
{
	[Post("/api/v1/state/{pipelineId}/config")]
	[Headers("Accept: application/json")]
	Task<ResponseDataFlowNodeConfigsDto> RetrieveNodeConfigsAsync(
		[AliasAs("pipelineId")] Guid pipelineId,
		[Header("Cookie")] AuthenticationCookie authenticationCookie,
		[Body] RequestDateFlowNodeConfigsDto requestNodeConfigsDto,
		CancellationToken cancellationToken = default);
}
