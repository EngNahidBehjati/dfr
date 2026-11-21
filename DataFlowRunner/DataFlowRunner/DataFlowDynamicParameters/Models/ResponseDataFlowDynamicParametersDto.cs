using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowDynamicParameters.Models;

public sealed record ResponseDataFlowDynamicParametersDto(
	[property: JsonPropertyName("pluginVariablesMap")] IReadOnlyCollection<ResponseNodeDynamicParametersDto> NodeDynamicParameters
);
