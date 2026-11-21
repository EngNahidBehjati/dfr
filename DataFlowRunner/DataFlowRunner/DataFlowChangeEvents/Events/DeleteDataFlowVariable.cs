using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowChangeEvents.Events;

public sealed record DeleteDataFlowVariable(
    [property: JsonPropertyName("id")] Guid Id
);
