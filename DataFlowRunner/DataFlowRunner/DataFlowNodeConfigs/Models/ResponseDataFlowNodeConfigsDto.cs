using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowNodeConfigs.Models;

public sealed record ResponseDataFlowNodeConfigsDto(
	[property: JsonPropertyName("exeConfigList")] List<ResponseNodeConfigDto> ResponseNodeConfigs
);
