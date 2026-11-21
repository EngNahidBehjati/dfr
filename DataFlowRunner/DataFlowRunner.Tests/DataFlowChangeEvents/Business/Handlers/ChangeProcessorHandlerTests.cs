using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Business.Handlers;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowChangeEvents.Business.Handlers;

public sealed class ChangeProcessorHandlerTests
{
	private readonly IDataFlowExecutionCommandPublisher _executionCommandPublisher;
	private readonly IDataFlowExecutionUserIdProvider _dataFlowExecutionUserIdProvider;
	private readonly ChangeProcessorHandler _sut;

	public ChangeProcessorHandlerTests()
	{
		_dataFlowExecutionUserIdProvider = Substitute.For<IDataFlowExecutionUserIdProvider>();
		_executionCommandPublisher = Substitute.For<IDataFlowExecutionCommandPublisher>();
		IServiceScopeFactory serviceScopeFactory = CreateServiceScopeFactory(_executionCommandPublisher);
		_sut = new ChangeProcessorHandler(serviceScopeFactory, _dataFlowExecutionUserIdProvider);
	}

	private static IServiceScopeFactory CreateServiceScopeFactory(
		IDataFlowExecutionCommandPublisher executionCommandPublisher)
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSingleton(executionCommandPublisher);
		ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[Fact]
	public async Task HandleAsync_ShouldPublishChangeDataFlowProcessorCommand_WhenCalled()
	{
		// Arrange
		var dataFlowExecutionId = DataFlowExecutionId.New();
		var runningVersion = new DataFlowVersion(5);
		var newVersion = new DataFlowVersion(10);
		IGraph graph = Substitute.For<IGraph>();
		ProcessorInfo processorInfo = DefaultProcessorInfoCreator.Create();
		var context = new DataFlowChangeContext(
			dataFlowExecutionId,
			runningVersion,
			newVersion,
			graph,
			processorInfo,
			DefaultExecutionSettingsCreator.Create());
		var processorId = Guid.NewGuid();
		var changeEvent = new ChangeProcessor(new DataFlowProcessorInfo("SOME_PROCESSOR", processorId));
		var userId = UserId.New();
		_dataFlowExecutionUserIdProvider.GetDataFlowExecutionUserId(dataFlowExecutionId).Returns(userId);

		var expectedCommand = new ChangeDataFlowProcessorCommand()
		{
			GraphExecutionId = dataFlowExecutionId.Value,
			UserId = userId.Value,
			ProcessorInfo = new ProcessorInfo()
			{
				ProcessorType = "SOME_PROCESSOR",
				ProcessorId = processorId
			}
		};

		// Act
		await _sut.HandleAsync(context, changeEvent, CancellationToken.None);

		// Assert
		await _executionCommandPublisher.Received(1).PublishCommandAsync(expectedCommand, CancellationToken.None);
		context.ProcessorInfo.Should().BeEquivalentTo(expectedCommand.ProcessorInfo);
	}
}
