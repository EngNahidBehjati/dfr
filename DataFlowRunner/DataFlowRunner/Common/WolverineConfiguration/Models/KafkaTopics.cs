namespace DataFlowRunner.Common.WolverineConfiguration.Models;

internal sealed record KafkaTopics
{
	public required string CommandBoundTopic { get; set; }
	public required string ReportBoundTopic { get; set; }
	public required string DataFlowManagementCommandBoundTopic { get; set; }
	public required string RetryExecutionTopic { get; set; }
	public required string DataFlowChangesTopic { get; set; }
}
