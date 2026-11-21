using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DataFlowRunner.DataFlowChangeEvents.Abstractions;

internal interface IActionTypeExtractor
{
	bool TryGetActionType(JsonElement action, [NotNullWhen(true)] out string? type);
}
