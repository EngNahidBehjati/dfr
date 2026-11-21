using DataFlowRunner.DataFlowDynamicParameters.Abstractions.ProvideRequiredDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Validation;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowDynamicParameters.Business.ProvideRequiredDynamicParameters;

internal sealed class DynamicParametersResponseResolver : IDynamicParametersResponseResolver
{
	private readonly INodeAncestryParameterValueExtractor _ancestryParameterValueExtractor;
	private readonly IDataFlowParametersValueExtractor _dataFlowParametersValueExtractor;
	private readonly IDynamicParameterTypeValidator _dynamicParameterTypeValidator;

	public DynamicParametersResponseResolver(
		INodeAncestryParameterValueExtractor ancestryValueExtractor,
		IDataFlowParametersValueExtractor dataFlowParametersValueExtractor, IDynamicParameterTypeValidator dynamicParameterTypeValidator)
	{
		_ancestryParameterValueExtractor = ancestryValueExtractor ?? throw new ArgumentNullException(nameof(ancestryValueExtractor));
		_dataFlowParametersValueExtractor = dataFlowParametersValueExtractor ?? throw new ArgumentNullException(nameof(dataFlowParametersValueExtractor));
		_dynamicParameterTypeValidator = dynamicParameterTypeValidator ?? throw new ArgumentNullException(nameof(dynamicParameterTypeValidator));
	}

	public List<DynamicParameterDto> ResolveForNode(
		DataFlowExecutionId executionId,
		NodeId targetNodeId,
		IReadOnlyGraph graph,
		ResponseDataFlowDynamicParametersDto dynamicParameters)
	{
		var expectedNames = dynamicParameters.NodeDynamicParameters
			.SelectMany(plugin => plugin.DynamicParameters)
			.Select(p => p.Name)
			.Distinct(StringComparer.Ordinal)
			.ToList();

		IReadOnlyCollection<DynamicParameter> resolved =
			_ancestryParameterValueExtractor.Extract(executionId, targetNodeId, graph, expectedNames);

		return MapToDto(resolved, dynamicParameters);
	}

	public List<DynamicParameterDto> ResolveForFlow(
		DataFlowExecutionId executionId,
		IReadOnlyGraph graph,
		ResponseDataFlowDynamicParametersDto dynamicParameters)
	{
		IReadOnlyCollection<DynamicParameter> resolved =
			_dataFlowParametersValueExtractor.Extract(executionId, graph);

		return MapToDto(resolved, dynamicParameters);
	}

	private List<DynamicParameterDto> MapToDto(
		IReadOnlyCollection<DynamicParameter> resolved,
		ResponseDataFlowDynamicParametersDto dynamicParameters)
	{
		var resolvedParametersByName = resolved.ToDictionary(p => p.Name);

		return dynamicParameters.NodeDynamicParameters
			.SelectMany(plugin => plugin.DynamicParameters)
			.DistinctBy(parameter => parameter.Id)
			.Select(parameter => new DynamicParameterDto
			{
				Id = parameter.Id,
				Name = parameter.Name,
				Required = parameter.Required,
				DataType = parameter.DataType,
				IsList = parameter.IsList,
				DefaultValue = GetParameterDefaultValue(resolvedParametersByName, parameter)
			})
			.ToList();
	}

	private object? GetParameterDefaultValue(Dictionary<string, DynamicParameter> resolvedByName,
		ResponseDataFlowDynamicParameterDto parameter)
	{
		if (!resolvedByName.TryGetValue(parameter.Name, out DynamicParameter? stored))
			return parameter.DefaultValue;

		return _dynamicParameterTypeValidator.IsConvertible(stored.Value, parameter.DataType, parameter.IsList)
			? stored.Value
			: parameter.DefaultValue;
	}
}
