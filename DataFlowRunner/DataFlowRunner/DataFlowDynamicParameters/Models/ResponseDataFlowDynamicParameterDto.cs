using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowDynamicParameters.Models;

public sealed record ResponseDataFlowDynamicParameterDto(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("name")] string Name,
	[property: JsonPropertyName("required")] bool Required,
	[property: JsonPropertyName("dataType")] string DataType,
	[property: JsonPropertyName("list")] bool IsList,
	[property: JsonPropertyName("defaultValue")] object? DefaultValue
);
