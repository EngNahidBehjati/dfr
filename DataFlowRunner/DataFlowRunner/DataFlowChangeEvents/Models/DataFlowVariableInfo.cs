using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowChangeEvents.Models;

public sealed record DataFlowVariableInfo(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("list")] bool IsList,
    [property: JsonPropertyName("required")] bool Required,
    [property: JsonPropertyName("dataType")] string DataType,
    [property: JsonPropertyName("defaultValue")] DefaultValueInfo? DefaultValueInfo
);
