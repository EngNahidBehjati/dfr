using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowChangeEvents.Models;

public sealed record DataFlowExecutionSettings(
	[property: JsonPropertyName("type")] string Type,
	[property: JsonPropertyName("executorId")] Guid? ExecutorId
);
