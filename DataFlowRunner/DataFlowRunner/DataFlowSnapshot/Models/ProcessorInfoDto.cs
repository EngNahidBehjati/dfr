using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowSnapshot.Models;

public sealed record ProcessorInfoDto(
	[property: JsonPropertyName("type")] string ProcessorType,
	[property: JsonPropertyName("processorId")] Guid? ProcessorId
);
