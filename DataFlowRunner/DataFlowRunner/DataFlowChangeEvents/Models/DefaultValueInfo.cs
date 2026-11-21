using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowChangeEvents.Models;

public sealed record DefaultValueInfo(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("value")] string Value
);
