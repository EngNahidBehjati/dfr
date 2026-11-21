namespace DataFlowRunner.DataFlowDynamicParameters.Models;

public sealed class DynamicParameterDto
{
	public required string Id { get; init; }
	public required string Name { get; init; }
	public required bool Required { get; init; }
	public required string DataType { get; init; }
	public required bool IsList { get; init; }
	public required object? DefaultValue { get; init; }
}
