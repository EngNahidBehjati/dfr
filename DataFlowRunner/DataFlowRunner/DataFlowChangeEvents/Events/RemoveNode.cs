using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowChangeEvents.Events;

public sealed record RemoveNode(
    [property: JsonPropertyName("id")] Guid Id
);
