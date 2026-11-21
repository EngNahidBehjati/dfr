using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowSnapshot.Models;

public sealed record ExecutionSettingsDto(
	[property: JsonPropertyName("type")] string ExecutorType,
	[property: JsonPropertyName("executorId")] Guid? ExecutorId
);
