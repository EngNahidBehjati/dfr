using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowChangeEvents.Models;

public sealed record DataFlowProcessorInfo(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("processorId")] Guid? ProcessorId
);
