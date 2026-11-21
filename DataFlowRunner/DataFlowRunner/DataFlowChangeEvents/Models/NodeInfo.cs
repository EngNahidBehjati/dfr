using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowChangeEvents.Models;

public sealed record NodeInfo(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("ports")] Dictionary<string, IEnumerable<PortInfo>> Ports
);
