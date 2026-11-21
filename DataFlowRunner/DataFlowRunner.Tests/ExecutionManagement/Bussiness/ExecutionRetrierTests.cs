using DataFlowRunner.Common.Authentication.Abstractions;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.ExecutionManagement.Abstractions;
using DataFlowRunner.ExecutionManagement.Business;
using DataFlowRunner.ExecutionManagement.Exceptions;
using DataFlowRunner.ExecutionManagement.Models;
using DataFlowRunner.Language.LanguageDictionary;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace DataFlowRunner.Tests.ExecutionManagement.Bussiness;

public sealed class ExecutionRetrierTests
{
	private readonly IRetryExecutionCommandInvoker _retryExecutionCommandInvoker;
	private readonly IAuthenticatedUserProvider _authenticatedUserProvider;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider = LanguageDictionaryProvider.Instance;
	private readonly ExecutionRetrier _sut;

	public ExecutionRetrierTests()
	{
		_retryExecutionCommandInvoker = Substitute.For<IRetryExecutionCommandInvoker>();
		_authenticatedUserProvider = Substitute.For<IAuthenticatedUserProvider>();
		ILogger<ExecutionRetrier> logger = Substitute.For<ILogger<ExecutionRetrier>>();
		_sut = new ExecutionRetrier(
			_retryExecutionCommandInvoker,
			_authenticatedUserProvider,
			_languageDictionaryProvider,
			logger);
	}

	[Fact]
	public async Task RetryExecutionAsync_ShouldInvokeRetryCommand_WhenSucceeds()
	{
		// Arrange
		var command = new RetryExecutionCommandDto
		{
			ExecutionSource = "API",
			DataFlowId = Guid.NewGuid(),
			DataFlowExecutionId = Guid.NewGuid(),
			DynamicParameters =
			[
				new RetryExecutionDynamicParameterDto { Name = "age", Value = 2 }
			]
		};
		CancellationToken cancellationToken = CancellationToken.None;

		var userId = Guid.NewGuid();
		_authenticatedUserProvider.GetCurrentUserId(cancellationToken).Returns(new UserId(userId));

		var expectedCommand = new RetryExecutionCommand
		{
			UserId = userId,
			DataFlowExecutionId = command.DataFlowExecutionId,
			DataFlowId = command.DataFlowId,
			ExecutionSource = command.ExecutionSource,
			DynamicParameters = command.DynamicParameters
		};
		_retryExecutionCommandInvoker.InvokeAsync(expectedCommand, cancellationToken).Returns(new RetryExecutionReport
		{
			DataFlowExecutionId = command.DataFlowExecutionId,
			IsSucceeded = true,
			NewDataFlowExecutionId = Guid.NewGuid(),
			ErrorMessage = null
		});

		// Act
		await _sut.RetryExecutionAsync(command, cancellationToken);

		// Assert
		await _retryExecutionCommandInvoker.Received(1).InvokeAsync(expectedCommand, cancellationToken);
	}

	[Fact]
	public async Task RetryExecutionAsync_ShouldThrowRetryExecutionFailedException_WhenInvokeFails()
	{
		// Arrange
		RetryExecutionCommandDto command = CreateCommandDto();
		CancellationToken cancellationToken = CancellationToken.None;
		_authenticatedUserProvider.GetCurrentUserId(cancellationToken).Returns(new UserId(Guid.NewGuid()));
		_retryExecutionCommandInvoker
			.InvokeAsync(Arg.Any<RetryExecutionCommand>(), cancellationToken)
			.ThrowsAsync(new TimeoutException());

		// Act
		Func<Task> act = async () => await _sut.RetryExecutionAsync(command, cancellationToken);

		// Assert
		await act.Should().ThrowAsync<RetryExecutionFailedException>()
			.WithMessage(_languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner
				.DataFlowExecution.RetryExecutionTimedOut());
	}

	[Fact]
	public async Task RetryExecutionAsync_ShouldThrowRetryExecutionFailedExceptionWithReportError_WhenValidationFails()
	{
		// Arrange
		RetryExecutionCommandDto command = CreateCommandDto();
		CancellationToken cancellationToken = CancellationToken.None;
		_authenticatedUserProvider.GetCurrentUserId(cancellationToken).Returns(new UserId(Guid.NewGuid()));
		_retryExecutionCommandInvoker
			.InvokeAsync(Arg.Any<RetryExecutionCommand>(), cancellationToken)
			.Returns(new RetryExecutionReport
			{
				DataFlowExecutionId = command.DataFlowExecutionId,
				IsSucceeded = false,
				NewDataFlowExecutionId = null,
				ErrorMessage = "validation failed"
			});

		var expectedMessage = _languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.DataFlowExecution
			.RetryExecutionFailed() + "\n\t- validation failed";

		// Act
		Func<Task> act = async () => await _sut.RetryExecutionAsync(command, cancellationToken);

		// Assert
		await act.Should().ThrowAsync<RetryExecutionFailedException>().WithMessage(expectedMessage);
	}

	[Fact]
	public async Task RetryExecutionAsync_ShouldThrowRetryExecutionFailedExceptionWithDictionaryMessage_WhenValidationFailsWithoutErrorMessage()
	{
		// Arrange
		RetryExecutionCommandDto command = CreateCommandDto();
		CancellationToken cancellationToken = CancellationToken.None;
		_authenticatedUserProvider.GetCurrentUserId(cancellationToken).Returns(new UserId(Guid.NewGuid()));
		_retryExecutionCommandInvoker
			.InvokeAsync(Arg.Any<RetryExecutionCommand>(), cancellationToken)
			.Returns(new RetryExecutionReport
			{
				DataFlowExecutionId = command.DataFlowExecutionId,
				IsSucceeded = false,
				NewDataFlowExecutionId = null,
				ErrorMessage = null
			});

		// Act
		Func<Task> act = async () => await _sut.RetryExecutionAsync(command, cancellationToken);

		// Assert
		await act.Should().ThrowAsync<RetryExecutionFailedException>()
			.WithMessage(_languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner
				.DataFlowExecution.RetryExecutionFailed());
	}

	private static RetryExecutionCommandDto CreateCommandDto()
	{
		return new RetryExecutionCommandDto
		{
			ExecutionSource = "API",
			DataFlowId = Guid.NewGuid(),
			DataFlowExecutionId = Guid.NewGuid(),
			DynamicParameters =
			[
				new RetryExecutionDynamicParameterDto { Name = "age", Value = 2 }
			]
		};
	}
}
