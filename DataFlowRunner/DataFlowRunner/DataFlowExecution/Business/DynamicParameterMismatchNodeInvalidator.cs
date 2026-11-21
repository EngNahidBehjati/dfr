using System.Text.Json;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class DynamicParameterMismatchNodeInvalidator : IDynamicParameterMismatchNodeInvalidator
{
	private readonly IDataFlowDynamicParametersStore _dataFlowDynamicParametersStore;
	private readonly IDataFlowInvalidator _dataFlowInvalidator;

	public DynamicParameterMismatchNodeInvalidator(
		IDataFlowDynamicParametersStore dataFlowDynamicParametersStore,
		IDataFlowInvalidator dataFlowInvalidator)
	{
		_dataFlowDynamicParametersStore = dataFlowDynamicParametersStore ??
			throw new ArgumentNullException(nameof(dataFlowDynamicParametersStore));
		_dataFlowInvalidator = dataFlowInvalidator ?? throw new ArgumentNullException(nameof(dataFlowInvalidator));
	}

	public async Task InvalidateNodesOnParameterMismatchAsync(
		DataFlowExecutionContext dataFlowExecutionContext,
		IReadOnlyGraph subgraphToCompare,
		List<DynamicParameter> providedDynamicParameters,
		CancellationToken cancellationToken)
	{
		DataFlowExecutionId dataFlowExecutionId = dataFlowExecutionContext.DataFlowExecutionId;
		if (providedDynamicParameters.Count == 0)
		{
			return;
		}

		IReadOnlyCollection<NodeId> conflictedUpstreamNodeIds = subgraphToCompare.GetAllNodes()
			.Select(node => node.Id)
			.Where(nodeId =>
				NodeContainsParameterWithDifferentValue(dataFlowExecutionId, nodeId, providedDynamicParameters))
			.ToList();

		foreach (NodeId conflictedUpstreamNodeId in conflictedUpstreamNodeIds)
		{
			await _dataFlowInvalidator
				.InvalidateNodeAndDownstreamAsync(dataFlowExecutionContext, conflictedUpstreamNodeId, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	private bool NodeContainsParameterWithDifferentValue(
		DataFlowExecutionId dataFlowExecutionId,
		NodeId nodeId,
		List<DynamicParameter> providedParameters)
	{
		if (!_dataFlowDynamicParametersStore.TryGetDynamicParameters(dataFlowExecutionId, nodeId,
			    out IReadOnlyCollection<DynamicParameter> parameters))
		{
			return false;
		}

		return parameters.Any(parameter =>
		{
			DynamicParameter? matchedParameter =
				providedParameters.FirstOrDefault(p => p.Name == parameter.Name);
			if (matchedParameter is null)
			{
				return false;
			}

			return !AreParameterValuesEqual(matchedParameter.Value, parameter.Value);
		});
	}

	private static bool AreParameterValuesEqual(object? providedValue, object? storedValue)
	{
		if (Equals(providedValue, storedValue))
		{
			return true;
		}

		if (providedValue is JsonElement providedElement && storedValue is JsonElement storedElement)
		{
			return providedElement.GetRawText() == storedElement.GetRawText();
		}

		return false;
	}
}
