using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using DataFlowRunner.DataFlowChangeEvents.Abstractions;

namespace DataFlowRunner.DataFlowChangeEvents.Business.Parsing;

internal sealed class ActionTypeExtractor : IActionTypeExtractor
{
	public bool TryGetActionType(JsonElement action, [NotNullWhen(true)] out string? type)
	{
		type = null;

		if (action.ValueKind != JsonValueKind.Object)
		{
			return false;
		}

		if (!action.TryGetProperty("type", out JsonElement typeElement))
		{
			return false;
		}

		if (typeElement.ValueKind != JsonValueKind.String)
		{
			return false;
		}

		type = typeElement.GetString();
		return !string.IsNullOrWhiteSpace(type);
	}
}
