using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.ExecutionManagement.Abstractions;
using DataFlowRunner.ExecutionManagement.Business;
using DataFlowRunner.ExecutionManagement.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DataFlowRunner.Tests.ExecutionManagement.Bussiness;

public class ExecutionCancellerTests
{
	private readonly ICancelExecutionCommandPublisher _cancelExecutionCommandPublisher;
	private readonly ILogger<ExecutionCanceller> _logger;
	private readonly IDataFlowExecutionFacade _dataFlowExecutionFacade;
	private readonly ExecutionCanceller _sut;

	public ExecutionCancellerTests()
	{
		_cancelExecutionCommandPublisher = Substitute.For<ICancelExecutionCommandPublisher>();
		_logger = Substitute.For<ILogger<ExecutionCanceller>>();
		_dataFlowExecutionFacade = Substitute.For<IDataFlowExecutionFacade>();

		_logger.BeginScope(Arg.Any<Dictionary<string, object>>())
			.Returns(Substitute.For<IDisposable>());

		_sut = new ExecutionCanceller(_cancelExecutionCommandPublisher, _logger, _dataFlowExecutionFacade);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenCancelExecutionCommandPublisherIsNull()
	{
		// Arrange
		ICancelExecutionCommandPublisher cancelExecutionCommandPublisher = null!;

		// Act
		Action act = () =>
			_ = new ExecutionCanceller(cancelExecutionCommandPublisher, _logger, _dataFlowExecutionFacade);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
	{
		// Arrange
		ILogger<ExecutionCanceller> logger = null!;

		// Act
		Action act = () =>
			_ = new ExecutionCanceller(_cancelExecutionCommandPublisher, logger, _dataFlowExecutionFacade);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowExecutionFacadeIsNull()
	{
		// Arrange
		IDataFlowExecutionFacade dataFlowExecutionFacade = null!;

		// Act
		Action act = () =>
			_ = new ExecutionCanceller(_cancelExecutionCommandPublisher, _logger, dataFlowExecutionFacade);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public async Task CancelExecutionAsync_ShouldCancelViaDataFlowExecutionFacade_WhenExecutionSourceIsWebDataFlowPage()
	{
		// Arrange
		var command = new CancelCommandDto
		{
			ExecutionSource = "WEB_DATA_FLOW_PAGE",
			DataFlowId = Guid.NewGuid(),
			DataFlowExecutionId = Guid.NewGuid()
		};
		CancellationToken cancellationToken = CancellationToken.None;

		_dataFlowExecutionFacade
			.CancelDataFlow(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(Task.CompletedTask);

		// Act
		await _sut.CancelExecutionAsync(command, cancellationToken);

		// Assert
		await _dataFlowExecutionFacade.Received(1)
			.CancelDataFlow(command.DataFlowExecutionId, cancellationToken);

		await _cancelExecutionCommandPublisher.DidNotReceive()
			.PublishCommandAsync(Arg.Any<CancelCommandDto>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CancelExecutionAsync_ShouldPublishCancelCommandAndNotCallDataFlowExecutionFacade_WhenExecutionSourceIsNotWebDataFlowPage()
	{
		// Arrange
		var command = new CancelCommandDto
		{
			ExecutionSource = "API",
			DataFlowId = Guid.NewGuid(),
			DataFlowExecutionId = Guid.NewGuid()
		};
		CancellationToken cancellationToken = CancellationToken.None;

		_cancelExecutionCommandPublisher
			.PublishCommandAsync(Arg.Any<CancelCommandDto>(), Arg.Any<CancellationToken>())
			.Returns(new ValueTask());

		// Act
		await _sut.CancelExecutionAsync(command, cancellationToken);

		// Assert
		await _cancelExecutionCommandPublisher.Received(1)
			.PublishCommandAsync(command, cancellationToken);

		await _dataFlowExecutionFacade.DidNotReceive()
			.CancelDataFlow(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
	}
}
