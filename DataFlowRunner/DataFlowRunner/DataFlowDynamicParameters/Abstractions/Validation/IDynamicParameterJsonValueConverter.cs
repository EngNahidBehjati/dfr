using System.Text.Json;

namespace DataFlowRunner.DataFlowDynamicParameters.Abstractions.Validation;

internal interface IDynamicParameterJsonValueConverter
{
	object? Convert(JsonElement value);
}
