namespace DataFlowRunner.DataFlowExecution.Dto;

public sealed class RunDataFlowDto
{
	public IReadOnlyCollection<DynamicParameterExecutionDto> DynamicParameters { get; init; } = [];
}
