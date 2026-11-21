using System.Text.Json.Serialization;

namespace DataFlowRunner.Common.Authorization.Models;

public sealed class HasServiceAccessesRequest
{
	[JsonPropertyName("targetSystemId")]
	public Guid TargetSystemId { get; init; }

	[JsonPropertyName("servicesAccesses")]
	public required List<ServiceAccessQuestionParameter> ServicesAccesses { get; init; }

	[JsonPropertyName("forbiddenServicesAccesses")]
	public required List<ServiceAccessQuestionParameter> ForbiddenServicesAccesses { get; init; }
}
