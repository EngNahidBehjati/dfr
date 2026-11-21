using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.GraphCore.Models.NodeModels;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecution.Business;

public class NodeCancellerTests
{
	private readonly IDataFlowExecutionCommandPublisher _executionCommandPublisher;
	private readonly IDataFlowExecutionUserIdProvider _dataFlowExecutionUserIdProvider;
	private readonly NodeCanceller _sut;

	public NodeCancellerTests()
	{
		_dataFlowExecutionUserIdProvider = Substitute.For<IDataFlowExecutionUserIdProvider>();
		_executionCommandPublisher = Substitute.For<IDataFlowExecutionCommandPublisher>();
		IServiceScopeFactory serviceScopeFactory = CreateServiceScopeFactory(_executionCommandPublisher);
		_sut = new NodeCanceller(serviceScopeFactory, _dataFlowExecutionUserIdProvider);
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
	public void Constructor_ShouldThrowArgumentNullException_WhenServiceScopeFactoryIsNull()
	{
		// Arrange
		IServiceScopeFactory serviceScopeFactory = null!;

		// Act
		Action act = () => _ = new NodeCanceller(serviceScopeFactory, _dataFlowExecutionUserIdProvider);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("serviceScopeFactory");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowExecutionUserIdProviderIsNull()
	{
		// Arrange
		IDataFlowExecutionUserIdProvider dataFlowExecutionUserIdProvider = null!;

		// Act
		Action act = () => _ = new NodeCanceller(CreateServiceScopeFactory(_executionCommandPublisher), dataFlowExecutionUserIdProvider);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("dataFlowExecutionUserIdProvider");
	}

	[Fact]
	public async Task CancelNodeAsync_ShouldSendCancelNodeCommand_Whenever()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		var userId = new UserId(Guid.NewGuid());
		_dataFlowExecutionUserIdProvider.GetDataFlowExecutionUserId(dataFlowExecutionId).Returns(userId);

		// Act
		await _sut.CancelNodeAsync(dataFlowExecutionId, nodeId, CancellationToken.None);

		// Assert
		await _executionCommandPublisher.Received(1).PublishCommandAsync(Arg.Is<CancelNodeCommand>
				(command => command.GraphExecutionId == dataFlowExecutionId.Value &&
							command.UserId == userId.Value &&
							command.NodeId == nodeId.Value),
			CancellationToken.None);
	}
}
