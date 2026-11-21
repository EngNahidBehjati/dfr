using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowChangeEvents.Events;

public sealed record ChangeEvent(
	[property: JsonPropertyName("schemaVersion")] int SchemaVersion,
	[property: JsonPropertyName("pipelineVersion")] long DataFlowVersion,
	[property: JsonPropertyName("actions")] IReadOnlyList<JsonElement> Actions
);
