using System.Globalization;
using System.Text.Json;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Validation;

namespace DataFlowRunner.DataFlowDynamicParameters.Business.Validation;

internal sealed class DynamicParameterTypeValidator : IDynamicParameterTypeValidator
{
	private readonly IDynamicParameterJsonValueConverter _jsonValueConverter;

	public DynamicParameterTypeValidator(IDynamicParameterJsonValueConverter jsonValueConverter)
	{
		_jsonValueConverter = jsonValueConverter ?? throw new ArgumentNullException(nameof(jsonValueConverter));
	}

	public bool IsConvertible(object? value, string dataType, bool isList)
	{
		var normalizedValue = NormalizeValue(value);

		return isList
			? IsListConvertible(normalizedValue, dataType)
			: IsScalarConvertible(normalizedValue, dataType);
	}

	private object? NormalizeValue(object? value)
	{
		if (value is JsonElement jsonElement)
			return _jsonValueConverter.Convert(jsonElement);
		return value;
	}

	private static bool IsListConvertible(object? value, string dataType)
	{
		if (value is not List<object?> list)
		{
			return false;
		}

		return list.All(item => IsScalarConvertible(item, dataType));
	}

	private static bool IsScalarConvertible(object? value, string dataType)
	{
		if (value is null)
		{
			return false;
		}

		return dataType.ToUpperInvariant() switch
		{
			"INTEGER" => value is long,
			"FLOAT" => value is long or double,
			"BOOLEAN" => value is bool,
			"DATE" => value is string text && IsParsableDate(text),
			"TIME" => value is string text && IsParsableTime(text),
			"DATETIME" => value is string text && IsParsableDateTime(text),
			"UUID" => value is string text && Guid.TryParse(text, out _),
			"TEXT" => value is string,
			_ => false
		};
	}

	private static bool IsParsableDate(string text) =>
		DateOnly.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
		|| DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

	private static bool IsParsableTime(string text) =>
		TimeOnly.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
		|| TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out _);

	private static bool IsParsableDateTime(string text) =>
		DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
}
