using DataFlowRunner.ExecutionManagement.Models;
using DataFlowRunner.ExecutionManagement.WolverineIntegration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Wolverine;

namespace DataFlowRunner.Tests.ExecutionManagement.WolverineIntegration;

public sealed class WolverineExecutionCancelCommandPublisherTests
{
	private readonly IMessageBus _messageBus;
	private readonly WolverineExecutionCancelCommandPublisher _sut;

	public WolverineExecutionCancelCommandPublisherTests()
	{
		_messageBus = Substitute.For<IMessageBus>();
		ILogger<WolverineExecutionCancelCommandPublisher> logger =
			Substitute.For<ILogger<WolverineExecutionCancelCommandPublisher>>();
		_sut = new WolverineExecutionCancelCommandPublisher(_messageBus, logger);
	}

	[Fact]
	public async Task PublishCommandAsync_ShouldSendCancelCommand_WhenMethodCalled()
	{
		// Arrange
		var command = new CancelCommandDto
		{
			ExecutionSource = "API",
			DataFlowId = Guid.NewGuid(),
			DataFlowExecutionId = Guid.NewGuid()
		};
		var expectedCommand = new CancelExecutionCommand
		{
			ExecutionSource = command.ExecutionSource,
			DataFlowId = command.DataFlowId,
			DataFlowExecutionId = command.DataFlowExecutionId
		};

		// Act
		await _sut.PublishCommandAsync(command, CancellationToken.None);

		// Assert
		await _messageBus.Received(1).SendAsync(expectedCommand);
	}
}
