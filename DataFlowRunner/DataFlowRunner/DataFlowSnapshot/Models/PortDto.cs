using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowSnapshot.Models;

[SuppressMessage("Style", "CA1801:Review unused parameters")]
public sealed record PortDto(
	[property: JsonPropertyName("id")] Guid Id,
	[property: JsonPropertyName("type")] PortDirectionDto Type
);
