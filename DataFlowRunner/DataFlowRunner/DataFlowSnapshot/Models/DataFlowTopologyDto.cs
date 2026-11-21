using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowSnapshot.Models;

[SuppressMessage("Style", "CA1801:Review unused parameters")]
public sealed record DataFlowTopologyDto(
	[property: JsonPropertyName("topology")] TopologyDto Topology,
	[property: JsonPropertyName("version")] long DataFlowVersion
);
