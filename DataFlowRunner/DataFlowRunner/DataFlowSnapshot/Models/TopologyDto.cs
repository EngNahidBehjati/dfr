using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowSnapshot.Models;

public sealed record TopologyDto(
	[property: JsonPropertyName("plugins")] IEnumerable<NodeDto> Nodes,
	[property: JsonPropertyName("links")] IEnumerable<EdgeDto> Edges,
	[property: JsonPropertyName("dataDestination")] ProcessorInfoDto ProcessorInfo,
	[property: JsonPropertyName("executionSettings")] ExecutionSettingsDto ExecutionSettings
);
