using System.Text.Json.Serialization;
using DataFlowRunner.DataFlowChangeEvents.Models;

namespace DataFlowRunner.DataFlowChangeEvents.Events;

public sealed record AddNode(
	[property: JsonPropertyName("plugin")] NodeInfo NodeInfo
);
