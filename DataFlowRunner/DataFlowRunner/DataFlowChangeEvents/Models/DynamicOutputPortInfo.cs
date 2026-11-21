using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowChangeEvents.Models;

public sealed record DynamicOutputPortInfo(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("title")] string Title
);
