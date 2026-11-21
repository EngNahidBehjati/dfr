using System.Text.Json.Serialization;
using DataFlowRunner.DataFlowChangeEvents.Models;

namespace DataFlowRunner.DataFlowChangeEvents.Events;

public sealed record AddEdge(
    [property: JsonPropertyName("link")] EdgeInfo EdgeInfo
);
