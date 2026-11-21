using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowSnapshot.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PortDirectionDto
{
	In,
	Out
}
