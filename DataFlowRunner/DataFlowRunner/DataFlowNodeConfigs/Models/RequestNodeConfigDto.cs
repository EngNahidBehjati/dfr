using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowNodeConfigs.Models;

public sealed record RequestNodeConfigDto(
	[property: JsonPropertyName("pluginId")] Guid NodeId,
	[property: JsonPropertyName("version")] string? Version
);
