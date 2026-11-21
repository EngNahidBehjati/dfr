using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;

namespace DataFlowRunner.DataFlowDynamicParameters.Abstractions.ProvideRequiredDynamicParameters;

/// <summary>
/// Extracts the effective stored dynamic parameters for an entire flow graph.
/// </summary>
internal interface IDataFlowParametersValueExtractor
{
	IReadOnlyCollection<DynamicParameter> Extract(
		DataFlowExecutionId executionId,
		IReadOnlyGraph graph);
}
