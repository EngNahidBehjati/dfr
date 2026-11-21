namespace DataFlowRunner.DataFlowExecution.Dto;

public sealed class RunDataFlowNodeDto
{
	public IReadOnlyCollection<DynamicParameterExecutionDto> DynamicParameters { get; init; } = [];
}
