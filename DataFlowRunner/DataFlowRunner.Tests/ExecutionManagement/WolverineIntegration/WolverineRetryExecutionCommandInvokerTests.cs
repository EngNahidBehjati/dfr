using DataFlowRunner.ExecutionManagement.Models;
using DataFlowRunner.ExecutionManagement.Options;
using DataFlowRunner.ExecutionManagement.WolverineIntegration;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Wolverine;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;

namespace DataFlowRunner.Tests.ExecutionManagement.WolverineIntegration;

public sealed class WolverineRetryExecutionCommandInvokerTests
{
	private readonly IOptions<RetryExecutionInvokeTimeoutOptions> _invokeTimeoutOptions;
	private readonly TimeSpan _expectedTimeout = TimeSpan.FromSeconds(60);
	private readonly ILogger<WolverineRetryExecutionCommandInvoker> _logger;

	public WolverineRetryExecutionCommandInvokerTests()
	{
		_invokeTimeoutOptions = MicrosoftOptions.Create(new RetryExecutionInvokeTimeoutOptions
		{
			TimeoutInSeconds = _expectedTimeout
		});
		_logger = Substitute.For<ILogger<WolverineRetryExecutionCommandInvoker>>();
	}

	[Fact]
	public async Task InvokeAsync_ShouldReturnReport_WhenCommandIsValid()
	{
		// Arrange
		RetryExecutionCommand command = CreateCommand();
		var expectedReport = new RetryExecutionReport
		{
			DataFlowExecutionId = command.DataFlowExecutionId,
			IsSucceeded = true,
			NewDataFlowExecutionId = Guid.NewGuid(),
			ErrorMessage = null
		};
		IServiceScopeFactory serviceScopeFactory = CreateServiceScopeFactory(command, expectedReport, _expectedTimeout);
		var sut = new WolverineRetryExecutionCommandInvoker(serviceScopeFactory, _invokeTimeoutOptions, _logger);

		// Act
		RetryExecutionReport result = await sut.InvokeAsync(command, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(expectedReport);
	}

	[Fact]
	public async Task InvokeAsync_ShouldRethrowException_WhenInvokeFails()
	{
		// Arrange
		RetryExecutionCommand command = CreateCommand();
		IServiceScopeFactory serviceScopeFactory = CreateFailingServiceScopeFactory(command, _expectedTimeout);
		var sut = new WolverineRetryExecutionCommandInvoker(serviceScopeFactory, _invokeTimeoutOptions, _logger);

		// Act
		Func<Task> act = async () => await sut.InvokeAsync(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<TimeoutException>();
	}

	private static RetryExecutionCommand CreateCommand()
	{
		return new RetryExecutionCommand
		{
			ExecutionSource = "API",
			DataFlowId = Guid.NewGuid(),
			DataFlowExecutionId = Guid.NewGuid(),
			UserId = Guid.NewGuid(),
			DynamicParameters =
			[
				new RetryExecutionDynamicParameterDto { Name = "age", Value = 2 }
			]
		};
	}

	private static IServiceScopeFactory CreateServiceScopeFactory(
		RetryExecutionCommand command, RetryExecutionReport expectedReport, TimeSpan expectedTimeout)
	{
		IMessageBus messageBus = Substitute.For<IMessageBus>();
		messageBus
			.InvokeAsync<RetryExecutionReport>(
				command,
				Arg.Any<CancellationToken>(),
				Arg.Is<TimeSpan>(t => Math.Abs((t - expectedTimeout).TotalMilliseconds) < 1))
			.Returns(Task.FromResult(expectedReport));

		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSingleton(messageBus);
		return serviceCollection.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
	}

	private static IServiceScopeFactory CreateFailingServiceScopeFactory(
		RetryExecutionCommand command, TimeSpan expectedTimeout)
	{
		IMessageBus messageBus = Substitute.For<IMessageBus>();
		messageBus
			.InvokeAsync<RetryExecutionReport>(
				command,
				Arg.Any<CancellationToken>(),
				Arg.Is<TimeSpan>(t => Math.Abs((t - expectedTimeout).TotalMilliseconds) < 1))
			.ThrowsAsync(new TimeoutException());

		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSingleton(messageBus);
		return serviceCollection.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
	}
}
