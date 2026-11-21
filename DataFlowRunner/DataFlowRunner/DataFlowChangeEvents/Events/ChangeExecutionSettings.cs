using System.Text.Json.Serialization;
using DataFlowRunner.DataFlowChangeEvents.Models;

namespace DataFlowRunner.DataFlowChangeEvents.Events;

public sealed record ChangeExecutionSettings(
	[property: JsonPropertyName("newExecutionSettings")] DataFlowExecutionSettings DataFlowExecutionSettings
);
