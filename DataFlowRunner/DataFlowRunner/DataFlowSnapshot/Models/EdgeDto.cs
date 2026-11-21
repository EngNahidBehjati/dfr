using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowSnapshot.Models;

[SuppressMessage("Style", "CA1801:Review unused parameters")]
public sealed record EdgeDto(
	[property: JsonPropertyName("id")] Guid EdgeId,
	[property: JsonPropertyName("from")] Guid FromPort,
	[property: JsonPropertyName("to")] Guid ToPort,
	[property: JsonPropertyName("fromPlugin")] Guid FromNode,
	[property: JsonPropertyName("toPlugin")] Guid ToNode
);
