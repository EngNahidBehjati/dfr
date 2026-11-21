using System.Text.Json.Serialization;
using DataFlowRunner.DataFlowChangeEvents.Models;

namespace DataFlowRunner.DataFlowChangeEvents.Events;

public sealed record SetNodeConfig(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("dynamicOutputPorts")] List<DynamicOutputPortInfo> DynamicOutputPorts
);
