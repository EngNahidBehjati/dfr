using System.Text.Json.Serialization;
using DataFlowRunner.DataFlowNodeConfigs.Business;

namespace DataFlowRunner.DataFlowNodeConfigs.Models;

public sealed record ResponseNodeConfigDto(
	[property: JsonPropertyName("pluginId")] Guid NodeId,
	[property: JsonPropertyName("version")] string Version,
	[property: JsonPropertyName("pluginExeJson"), JsonConverter(typeof(RawJsonStringConverter))] string Config
);
