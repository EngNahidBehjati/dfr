namespace DataFlowRunner.DataFlowDynamicParameters.Abstractions.Validation;

internal interface IDynamicParameterTypeValidator
{
	bool IsConvertible(object? value, string dataType, bool isList);
}
