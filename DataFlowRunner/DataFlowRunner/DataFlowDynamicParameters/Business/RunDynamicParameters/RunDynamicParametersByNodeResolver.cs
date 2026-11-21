using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Infrastructure;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.RunDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowDynamicParameters.Business.RunDynamicParameters;

internal sealed class RunDynamicParametersByNodeResolver : IRunDynamicParametersByNodeResolver
{
	private readonly IDynamicParametersFetcher _dynamicParametersFetcher;
	private readonly IRunDynamicParametersValidator _runDynamicParametersValidator;

	public RunDynamicParametersByNodeResolver(
		IDynamicParametersFetcher dynamicParametersFetcher,
		IRunDynamicParametersValidator runDynamicParametersValidator)
	{
		_dynamicParametersFetcher = dynamicParametersFetcher
		                            ?? throw new ArgumentNullException(nameof(dynamicParametersFetcher));
		_runDynamicParametersValidator = runDynamicParametersValidator
		                                 ?? throw new ArgumentNullException(nameof(runDynamicParametersValidator));
	}

	public async Task<IReadOnlyDictionary<NodeId, List<DynamicParameter>>> ResolveProvidedParametersByNodeAsync(
		DataFlowExecutionId executionId,
		IReadOnlyCollection<Node> nodes,
		List<DynamicParameter> providedDynamicParameters,
		CancellationToken cancellationToken)
	{
		var nodeIds = nodes.Select(node => node.Id).ToList();

		ResponseDataFlowDynamicParametersDto fetchResponse = await _dynamicParametersFetcher
			.FetchDataFlowDynamicParametersAsync(executionId, nodeIds, cancellationToken)
			.ConfigureAwait(false);

		_runDynamicParametersValidator.EnsureProvidedParametersAreValid(fetchResponse, providedDynamicParameters);

		return AssignProvidedParametersPerNode(nodes, providedDynamicParameters, fetchResponse);
	}

	private IReadOnlyDictionary<NodeId, List<DynamicParameter>> AssignProvidedParametersPerNode(
		IReadOnlyCollection<Node> nodes,
		IReadOnlyCollection<DynamicParameter> provided,
		ResponseDataFlowDynamicParametersDto fetchResponse)
	{
		var providedByName = provided
			.ToDictionary(p => p.Name);

		var paramsByNode = fetchResponse.NodeDynamicParameters
			.ToDictionary(
				n => new NodeId(n.NodeId),
				n => n.DynamicParameters.Select(p => p.Name)
					.ToHashSet());

		return nodes.ToDictionary(
			node => node.Id,
			node => paramsByNode.TryGetValue(node.Id, out HashSet<string>? names)
				? names.Where(providedByName.ContainsKey).Select(n => providedByName[n]).ToList()
				: []);
	}
}
