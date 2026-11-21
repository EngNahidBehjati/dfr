using System.Text.Json;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Validation;

namespace DataFlowRunner.DataFlowDynamicParameters.Business.Validation;

internal sealed class DynamicParameterJsonValueConverter : IDynamicParameterJsonValueConverter
{
	public object? Convert(JsonElement value)
	{
		return value.ValueKind switch
		{
			JsonValueKind.True => true,
			JsonValueKind.False => false,
			JsonValueKind.Number when value.TryGetInt64(out long integer) => integer,
			JsonValueKind.Number => value.GetDouble(),
			JsonValueKind.String => value.GetString(),
			JsonValueKind.Array => value.EnumerateArray()
				.Select(element => Convert(element))
				.ToList(),
			_ => value
		};
	}
}
