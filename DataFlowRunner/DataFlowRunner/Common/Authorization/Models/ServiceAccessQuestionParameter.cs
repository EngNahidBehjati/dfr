using System.Text.Json.Serialization;

namespace DataFlowRunner.Common.Authorization.Models;

public class ServiceAccessQuestionParameter
{
	[JsonPropertyName("ServiceId")] public Guid ServiceId { get; init; }
	[JsonPropertyName("AccessLevels")] public List<Guid> AccessLevels { get; init; } = [];
}
