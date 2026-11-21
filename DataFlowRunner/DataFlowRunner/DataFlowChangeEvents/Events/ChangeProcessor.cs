using System.Text.Json.Serialization;
using DataFlowRunner.DataFlowChangeEvents.Models;

namespace DataFlowRunner.DataFlowChangeEvents.Events;

public sealed record ChangeProcessor(
    [property: JsonPropertyName("newDataDestination")] DataFlowProcessorInfo DataFlowProcessorInfo
);
