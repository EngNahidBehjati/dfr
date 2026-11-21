using System.Text.Json.Serialization;
using DataFlowRunner.DataFlowChangeEvents.Models;

namespace DataFlowRunner.DataFlowChangeEvents.Events;

public sealed record UpdateDataFlowVariable(
    [property: JsonPropertyName("dataFlowVariable")] DataFlowVariableInfo DataFlowVariable
);
