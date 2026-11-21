using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecution.Business;

public class NodeExecutorEngineGraphInitializerTests
{
	private readonly IProcessorInfoProvider _processorInfoProvider;
	private readonly INodeExecutorEngineGraphInitializationTracker _initializationTracker;
	private readonly IDataFlowExecutionUserIdProvider _dataFlowExecutionUserIdProvider;
	private readonly IDataFlowExecutionCommandPublisher _executionCommandPublisher;
	private readonly IServiceScopeFactory _serviceScopeFactory;

	private readonly NodeExecutorEngineGraphInitializer _sut;

	public NodeExecutorEngineGraphInitializerTests()
	{
		_processorInfoProvider = Substitute.For<IProcessorInfoProvider>();
		_initializationTracker = Substitute.For<INodeExecutorEngineGraphInitializationTracker>();
		_dataFlowExecutionUserIdProvider = Substitute.For<IDataFlowExecutionUserIdProvider>();
		_executionCommandPublisher = Substitute.For<IDataFlowExecutionCommandPublisher>();
		_serviceScopeFactory = CreateServiceScopeFactory(_executionCommandPublisher);

		_sut = new NodeExecutorEngineGraphInitializer(_processorInfoProvider,
			_initializationTracker, _serviceScopeFactory, _dataFlowExecutionUserIdProvider);
	}

	private static IServiceScopeFactory CreateServiceScopeFactory(
		IDataFlowExecutionCommandPublisher executionCommandPublisher)
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSingleton(executionCommandPublisher);
		ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
		IServiceScopeFactory scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		return scopeFactory;
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenProcessorInfoProviderIsNull()
	{
		// Arrange
		IProcessorInfoProvider processorInfoProvider = null!;

		// Act
		Action act = () =>
			_ = new NodeExecutorEngineGraphInitializer(processorInfoProvider,
				_initializationTracker, _serviceScopeFactory, _dataFlowExecutionUserIdProvider);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("processorInfoProvider");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenInitializationTrackerIsNull()
	{
		// Arrange
		INodeExecutorEngineGraphInitializationTracker initializationTracker = null!;

		// Act
		Action act = () =>
			_ = new NodeExecutorEngineGraphInitializer(_processorInfoProvider,
				initializationTracker, _serviceScopeFactory, _dataFlowExecutionUserIdProvider);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("nodeExecutorEngineGraphInitializationTracker");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenServiceScopeFactoryIsNull()
	{
		// Arrange
		IServiceScopeFactory serviceScopeFactory = null!;

		// Act
		Action act = () =>
			_ = new NodeExecutorEngineGraphInitializer(_processorInfoProvider,
				_initializationTracker, serviceScopeFactory, _dataFlowExecutionUserIdProvider);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("serviceScopeFactory");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenUserProviderIsNull()
	{
		// Arrange
		IDataFlowExecutionUserIdProvider dataFlowExecutionUserIdProvider = null!;

		// Act
		Action act = () =>
			_ = new NodeExecutorEngineGraphInitializer(_processorInfoProvider,
				_initializationTracker, _serviceScopeFactory, dataFlowExecutionUserIdProvider);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("dataFlowExecutionUserIdProvider");
	}

	[Fact]
	public async Task TryInitialize_ShouldDoNothing_WhenDataFlowWasInitialized()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		_initializationTracker.IsDataFlowInitialized(dataFlowExecutionId).Returns(true);

		// Act
		await _sut.TryInitialize(dataFlowExecutionId, CancellationToken.None);

		// Assert
		_initializationTracker.DidNotReceive().TrackDataFlowInitialization(Arg.Any<DataFlowExecutionId>());
		await _executionCommandPublisher.DidNotReceiveWithAnyArgs()
			.PublishCommandAsync(null!, CancellationToken.None);
	}

	[Fact]
	public async Task TryInitialize_ShouldPublishInitializeCommand_WhenInitializationSucceeds()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());

		var processorInfo = new ProcessorInfo
		{
			ProcessorType = "SYSTEM_DEFAULT_PROCESSOR",
			ProcessorId = Guid.NewGuid()
		};
		_processorInfoProvider.GetProcessorInfoAsync(dataFlowExecutionId, CancellationToken.None).Returns(processorInfo);

		_initializationTracker.IsDataFlowInitialized(dataFlowExecutionId).Returns(false);

		var userId = new UserId(Guid.NewGuid());
		_dataFlowExecutionUserIdProvider.GetDataFlowExecutionUserId(dataFlowExecutionId).Returns(userId);

		// Act
		await _sut.TryInitialize(dataFlowExecutionId, CancellationToken.None);

		// Assert
		await _executionCommandPublisher.Received(1).PublishCommandAsync(
			Arg.Is<InitializeDataFlowExecutionCommand>(command =>
				command.GraphExecutionId == dataFlowExecutionId.Value &&
				command.UserId == userId.Value &&
				command.ProcessorInfo == processorInfo),
			CancellationToken.None);
	}
}
