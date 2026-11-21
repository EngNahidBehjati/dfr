using System.Text.Json.Serialization;
using DataFlowRunner.DataFlowChangeEvents.Models;

namespace DataFlowRunner.DataFlowChangeEvents.Events;

public sealed record CreateDataFlowVariable(
    [property: JsonPropertyName("dataFlowVariable")] DataFlowVariableInfo DataFlowVariableInfo
);
