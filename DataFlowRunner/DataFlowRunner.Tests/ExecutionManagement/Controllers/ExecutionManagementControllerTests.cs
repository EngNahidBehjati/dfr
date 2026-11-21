using DataFlowRunner.Common.Authorization.Abstraction;
using DataFlowRunner.ExecutionManagement.Abstractions;
using DataFlowRunner.ExecutionManagement.Controller;
using DataFlowRunner.ExecutionManagement.Models;
using DataFlowRunner.Language.LanguageDictionary;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace DataFlowRunner.Tests.ExecutionManagement.Controllers;

public class ExecutionManagementControllerTests
{
	private readonly IExecutionCanceller _executionCanceller;
	private readonly IExecutionRetrier _executionRetrier;
	private readonly IAdminAuthorization _adminAuthorization;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;
	private readonly ExecutionManagementController _sut;

	public ExecutionManagementControllerTests()
	{
		_executionCanceller = Substitute.For<IExecutionCanceller>();
		_executionRetrier = Substitute.For<IExecutionRetrier>();
		_languageDictionaryProvider = LanguageDictionaryProvider.Instance;
		_adminAuthorization = Substitute.For<IAdminAuthorization>();
		_sut = new ExecutionManagementController(_executionCanceller, _executionRetrier,
			_adminAuthorization, _languageDictionaryProvider);
	}

	[Fact]
	public async Task CancelExecution_ShouldReturnForbidden_WhenUserIsNotAdmin()
	{
		// Arrange
		_adminAuthorization.UserIsAdmin().Returns(false);
		var command = new CancelCommandDto
		{
			ExecutionSource = "API",
			DataFlowId = Guid.NewGuid(),
			DataFlowExecutionId = Guid.NewGuid()
		};
		// Act
		IActionResult result = await _sut.CancelExecution(command, CancellationToken.None);

		// Assert
		ObjectResult forbiddenResult = result.Should()
			.BeOfType<ObjectResult>()
			.Subject;

		forbiddenResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
		forbiddenResult.Value.Should().BeEquivalentTo(CreateExpectedForbiddenProblemDetails());

		await _adminAuthorization.Received(1).UserIsAdmin();
		await _executionCanceller.DidNotReceive()
			.CancelExecutionAsync(Arg.Any<CancelCommandDto>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CancelExecution_ShouldReturnBadRequest_WhenExecutionSourceIsInvalid()
	{
		// Arrange
		_adminAuthorization.UserIsAdmin().Returns(true);
		var command = new CancelCommandDto
		{
			ExecutionSource = "INVALID_SOURCE",
			DataFlowId = Guid.NewGuid(),
			DataFlowExecutionId = Guid.NewGuid()
		};
		// Act
		IActionResult result = await _sut.CancelExecution(command, CancellationToken.None);

		// Assert
		ObjectResult badRequestResult = result.Should()
			.BeOfType<ObjectResult>()
			.Subject;

		badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
		badRequestResult.Value.Should().BeEquivalentTo(CreateExpectedInvalidExecutionSourceProblemDetails());

		await _adminAuthorization.Received(1).UserIsAdmin();
		await _executionCanceller.DidNotReceive()
			.CancelExecutionAsync(Arg.Any<CancelCommandDto>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CancelExecution_ShouldCallExecutionCancellerAndReturnOk_WhenRequestIsValidAndUserIsAdmin()
	{
		// Arrange
		_adminAuthorization.UserIsAdmin().Returns(true);
		var command = new CancelCommandDto
		{
			ExecutionSource = "API",
			DataFlowId = Guid.NewGuid(),
			DataFlowExecutionId = Guid.NewGuid()
		};
		CancellationToken cancellationToken = CancellationToken.None;

		// Act
		IActionResult result = await _sut.CancelExecution(command, cancellationToken);

		// Assert
		result.Should().BeOfType<OkResult>();
		await _adminAuthorization.Received(1).UserIsAdmin();
		await _executionCanceller.Received(1).CancelExecutionAsync(command, cancellationToken);
	}

	[Fact]
	public async Task RetryExecution_ShouldReturnForbidden_WhenUserIsNotAdmin()
	{
		// Arrange
		_adminAuthorization.UserIsAdmin().Returns(false);
		var command = new RetryExecutionCommandDto
		{
			ExecutionSource = "API",
			DataFlowId = Guid.NewGuid(),
			DataFlowExecutionId = Guid.NewGuid(),
			DynamicParameters = new List<RetryExecutionDynamicParameterDto>
			{
				new() { Name = "age", Value = 2 }
			}
		};

		// Act
		IActionResult result = await _sut.RetryExecution(command, CancellationToken.None);

		// Assert
		ObjectResult forbiddenResult = result.Should()
			.BeOfType<ObjectResult>()
			.Subject;

		forbiddenResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
		forbiddenResult.Value.Should().BeEquivalentTo(CreateExpectedForbiddenProblemDetails());

		await _adminAuthorization.Received(1).UserIsAdmin();
		await _executionRetrier.DidNotReceive()
			.RetryExecutionAsync(Arg.Any<RetryExecutionCommandDto>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task RetryExecution_ShouldReturnBadRequest_WhenExecutionSourceIsInvalid()
	{
		// Arrange
		_adminAuthorization.UserIsAdmin().Returns(true);
		var command = new RetryExecutionCommandDto
		{
			ExecutionSource = "Invalid_SOURCE",
			DataFlowId = Guid.NewGuid(),
			DataFlowExecutionId = Guid.NewGuid(),
			DynamicParameters = new List<RetryExecutionDynamicParameterDto>
			{
				new() { Name = "age", Value = 2 }
			}
		};

		// Act
		IActionResult result = await _sut.RetryExecution(command, CancellationToken.None);

		// Assert
		ObjectResult badRequestResult = result.Should()
			.BeOfType<ObjectResult>()
			.Subject;

		badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
		badRequestResult.Value.Should().BeEquivalentTo(CreateExpectedInvalidExecutionSourceProblemDetails());

		await _adminAuthorization.Received(1).UserIsAdmin();
		await _executionRetrier.DidNotReceive()
			.RetryExecutionAsync(Arg.Any<RetryExecutionCommandDto>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task RetryExecution_ShouldCallExecutionRetrierAndReturnOk_WhenRequestIsValidAndUserIsAdmin()
	{
		// Arrange
		_adminAuthorization.UserIsAdmin().Returns(true);
		var command = new RetryExecutionCommandDto
		{
			ExecutionSource = "API",
			DataFlowId = Guid.NewGuid(),
			DataFlowExecutionId = Guid.NewGuid(),
			DynamicParameters = new List<RetryExecutionDynamicParameterDto>
			{
				new() { Name = "age", Value = 2 }
			}
		};
		CancellationToken cancellationToken = CancellationToken.None;

		// Act
		IActionResult result = await _sut.RetryExecution(command, cancellationToken);

		// Assert
		result.Should().BeOfType<OkResult>();
		await _adminAuthorization.Received(1).UserIsAdmin();
		await _executionRetrier.Received(1).RetryExecutionAsync(command, cancellationToken);
	}

	private ProblemDetails CreateExpectedForbiddenProblemDetails() =>
		new()
		{
			Title = _languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.Common.Forbidden(),
			Status = StatusCodes.Status403Forbidden,
			Type = "Forbidden"
		};

	private ProblemDetails CreateExpectedInvalidExecutionSourceProblemDetails() =>
		new()
		{
			Title = _languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.DataFlowExecution
				.ExecutionSourceIsNotValid(),
			Status = StatusCodes.Status400BadRequest,
			Type = "Bad Request"
		};
}
