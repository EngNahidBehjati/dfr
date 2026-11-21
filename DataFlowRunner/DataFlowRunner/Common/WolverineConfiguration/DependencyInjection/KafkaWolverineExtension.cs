using Confluent.Kafka;
using DataFlowRunner.Common.WolverineConfiguration.Models;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Abstractions;
using DataFlowRunner.ExecutionManagement.Models;
using DataFlowRunner.TableQuery.WolverineIntegration.Abstractions;
using Microsoft.Extensions.Options;
using Wolverine;
using Wolverine.Kafka;

namespace DataFlowRunner.Common.WolverineConfiguration.DependencyInjection;

internal sealed class KafkaWolverineExtension : IWolverineExtension
{
	private readonly string _kafkaBootstrap;
	private readonly string _commandBoundTopic;
	private readonly string _reportBoundTopic;
	private readonly string _dataFlowManagementCommandBoundTopic;
	private readonly string _retryExecutionTopic;
	private readonly string _dataFlowChangesTopic;

	public KafkaWolverineExtension(IOptions<KafkaBootstrap> kafkaBootstrap, IOptions<KafkaTopics> kafkaTopics)
	{
		ArgumentNullException.ThrowIfNull(kafkaBootstrap);
		ArgumentNullException.ThrowIfNull(kafkaTopics);

		_kafkaBootstrap = kafkaBootstrap.Value.Address;
		_commandBoundTopic = kafkaTopics.Value.CommandBoundTopic;
		_reportBoundTopic = kafkaTopics.Value.ReportBoundTopic;
		_dataFlowManagementCommandBoundTopic = kafkaTopics.Value.DataFlowManagementCommandBoundTopic;
		_retryExecutionTopic = kafkaTopics.Value.RetryExecutionTopic;
		_dataFlowChangesTopic = kafkaTopics.Value.DataFlowChangesTopic;
	}

	public void Configure(WolverineOptions options)
	{
		if (string.IsNullOrEmpty(_kafkaBootstrap)) return;

		options.UseKafka(_kafkaBootstrap)
			.ConfigureProducers(producer =>
			{
				producer.CompressionType = CompressionType.Gzip;
				producer.MessageMaxBytes = 10_000_000;
			})
			.AutoProvision();
		options.ListenToKafkaTopic(_reportBoundTopic).ProcessInline().UseForReplies()
			.ConfigureConsumer(consumer =>
			{
				consumer.GroupId = $"dataflow-report-consumer-{Guid.NewGuid()}";
				consumer.AutoOffsetReset = AutoOffsetReset.Latest;
				consumer.BootstrapServers = _kafkaBootstrap;
			});
		options.ListenToKafkaTopic(_dataFlowChangesTopic).ProcessInline().DefaultIncomingMessage<ChangeEvent>()
			.ConfigureConsumer(consumer =>
			{
				consumer.GroupId = $"dataflow-changes-consumer-{Guid.NewGuid()}";
				consumer.AutoOffsetReset = AutoOffsetReset.Latest;
				consumer.BootstrapServers = _kafkaBootstrap;
			});

		options.PublishMessage<CancelExecutionCommand>()
			.ToKafkaTopic(_dataFlowManagementCommandBoundTopic);
		options.PublishMessage<RetryExecutionCommand>()
			.ToKafkaTopic(_retryExecutionTopic);
		options.Publish(rule =>
		{
			rule.MessagesImplementing<IWolverineExecutionCommand>();
			rule.MessagesImplementing<IWolverineQueryCommand>();
			rule.ToKafkaTopic(_commandBoundTopic);
		});
	}
}
