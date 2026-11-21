using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowDynamicParameters.Models;

public sealed record ResponseNodeDynamicParametersDto(
	[property: JsonPropertyName("pluginId")] Guid NodeId,
	[property: JsonPropertyName("variablesList")] IReadOnlyCollection<ResponseDataFlowDynamicParameterDto> DynamicParameters
);
