using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowChangeEvents.Models;

public sealed record PortInfo(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("uniqueName")] string? UniqueName,
    [property: JsonPropertyName("title")] string Title
);
