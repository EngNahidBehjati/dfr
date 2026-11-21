namespace DataFlowRunner.DataFlowDynamicParameters.Models;

internal sealed class DynamicParameter
{
	public required string Name { get; init; }
	public required object? Value { get; init; }
}
