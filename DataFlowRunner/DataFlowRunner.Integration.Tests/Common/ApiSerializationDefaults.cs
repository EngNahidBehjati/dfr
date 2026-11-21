using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataFlowRunner.Integration.Tests.Common;

public static class ApiSerializationDefaults
{
	public static readonly JsonSerializerOptions Options = new()
	{
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() }
	};
}
