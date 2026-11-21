using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowSnapshot.Models;

[SuppressMessage("Style", "CA1801:Review unused parameters")]
public sealed record NodeDto(
	[property: JsonPropertyName("id")] Guid NodeId,
	[property: JsonPropertyName("type")] string NodeType,
	[property: JsonPropertyName("ports")] IEnumerable<PortDto> Ports,
	[property: JsonPropertyName("version")] long Version
);
