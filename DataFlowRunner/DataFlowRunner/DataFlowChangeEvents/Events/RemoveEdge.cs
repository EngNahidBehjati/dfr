using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowChangeEvents.Events;

public sealed record RemoveEdge(
    [property: JsonPropertyName("id")] Guid Id
);
