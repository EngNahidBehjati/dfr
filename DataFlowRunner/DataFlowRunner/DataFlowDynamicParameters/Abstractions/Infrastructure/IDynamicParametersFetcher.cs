using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowDynamicParameters.Abstractions.Infrastructure;

internal interface IDynamicParametersFetcher
{
	Task<ResponseDataFlowDynamicParametersDto> FetchDataFlowDynamicParametersAsync(DataFlowExecutionId dataFlowExecutionId,
		List<NodeId> nodeIds,
		CancellationToken cancellationToken);
}
