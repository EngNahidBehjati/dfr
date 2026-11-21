using DataFlowRunner.DataFlowDynamicParameters.Models;

namespace DataFlowRunner.DataFlowDynamicParameters.Abstractions.ProvideRequiredDynamicParameters;

public interface IRequiredDynamicParametersProvider
{
	Task<List<DynamicParameterDto>> ProvideDataFlowDynamicParametersAsync(Guid dataFlowExecutionId,
		long dataFlowVersion,
		CancellationToken cancellationToken);

	Task<List<DynamicParameterDto>> ProvideDataFlowDynamicParametersForRunningNodeAsync(Guid dataFlowExecutionId,
		Guid nodeId,
		long dataFlowVersion,
		CancellationToken cancellationToken);
}
