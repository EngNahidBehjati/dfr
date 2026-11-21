using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowDynamicParameters.Models;

public sealed record RequestDataFlowDynamicParametersDto(
	[property: JsonPropertyName("pluginIdList")] IReadOnlyCollection<Guid> NodeIds
);
