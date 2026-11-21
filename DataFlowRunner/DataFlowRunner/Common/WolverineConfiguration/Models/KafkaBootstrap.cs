namespace DataFlowRunner.Common.WolverineConfiguration.Models;

internal sealed record KafkaBootstrap
{
	public required string Address { get; set; }
}
