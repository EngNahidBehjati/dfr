using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowChangeEvents.Models;

public sealed record EdgeInfo(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("from")] Guid From,
    [property: JsonPropertyName("to")] Guid To,
    [property: JsonPropertyName("fromPlugin")] Guid FromNode,
    [property: JsonPropertyName("toPlugin")] Guid ToNode
);
