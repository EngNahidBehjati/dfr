using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.Common.WolverineConfiguration.Models;
using Microsoft.Extensions.Options;
using Wolverine;

namespace DataFlowRunner.Common.WolverineConfiguration.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddSingleton<IWolverineExtension, KafkaWolverineExtension>();
		services.AddSingleton<IValidateOptions<KafkaBootstrap>, KafkaBootstrapValidator>();
		services.AddOptions<KafkaTopics>().Configure<IConfiguration>((kafkaTopics, configuration) =>
		{
			var prefix = configuration.GetValue<string>("AppSettings:Kafka:TopicPrefix") ?? string.Empty;
			kafkaTopics.CommandBoundTopic = prefix + "dataflow-command";
			kafkaTopics.ReportBoundTopic = prefix + "dataflow-report";
			kafkaTopics.DataFlowManagementCommandBoundTopic = prefix + "dataflow-management-command";
			kafkaTopics.RetryExecutionTopic = prefix + "retry-execution";
			kafkaTopics.DataFlowChangesTopic = prefix + "pipeline-changes";
		}).ValidateOnStart();

		services.AddOptions<KafkaBootstrap>().Configure<IConfiguration>((opt, configuration) =>
		{
			var address =
				configuration.GetValue<string>("AppSettings:NodeExecutorEngine:DataFlowExecutionKafkaBootstrap") ??
				configuration.GetValue<string>("AppSettings:Kafka:BootstrapServer") ??
				string.Empty;
			opt.Address = address;
		}).ValidateOnStart();
	}
}
