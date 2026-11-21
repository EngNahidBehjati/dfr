using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowNodeConfigs.Models;

public sealed record RequestDateFlowNodeConfigsDto(
	[property: JsonPropertyName("configVersionList")] List<RequestNodeConfigDto> RequestNodeConfigs
);
