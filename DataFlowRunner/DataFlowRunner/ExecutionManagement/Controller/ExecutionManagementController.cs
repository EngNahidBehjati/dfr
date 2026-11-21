using Asp.Versioning;
using DataFlowRunner.Common.Authorization.Abstraction;
using DataFlowRunner.ExecutionManagement.Abstractions;
using DataFlowRunner.ExecutionManagement.Models;
using DataFlowRunner.Language.LanguageDictionary;
using Microsoft.AspNetCore.Mvc;

namespace DataFlowRunner.ExecutionManagement.Controller;

[ApiController]
[Route("api/execution-management")]
[ApiVersion("1")]
public sealed class ExecutionManagementController : ControllerBase
{
	private readonly IExecutionCanceller _executionCanceller;
	private readonly IExecutionRetrier _executionRetrier;
	private readonly IAdminAuthorization _adminAuthorization;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public ExecutionManagementController(IExecutionCanceller executionCanceller,
		IExecutionRetrier executionRetrier,
		IAdminAuthorization adminAuthorization,
		ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_executionCanceller = executionCanceller ?? throw new ArgumentNullException(nameof(executionCanceller));
		_executionRetrier = executionRetrier ?? throw new ArgumentNullException(nameof(executionRetrier));
		_adminAuthorization = adminAuthorization ?? throw new ArgumentNullException(nameof(adminAuthorization));
		_languageDictionaryProvider = languageDictionaryProvider ??
		                              throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	[HttpPost("cancel")]
	public async Task<IActionResult> CancelExecution(
		[FromBody] CancelCommandDto commandDto,
		CancellationToken cancellationToken)
	{
		if (!await _adminAuthorization.UserIsAdmin().ConfigureAwait(false))
		{
			return Problem(
				title: _languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.Common.Forbidden(),
				statusCode: StatusCodes.Status403Forbidden,
				type: "Forbidden");
		}

		if (!ExecutionSources.ValidExecutionSources.Contains(commandDto.ExecutionSource))
		{
			return Problem(
				title: _languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.DataFlowExecution
					.ExecutionSourceIsNotValid(),
				statusCode: StatusCodes.Status400BadRequest,
				type: "Bad Request");
		}

		await _executionCanceller.CancelExecutionAsync(commandDto, cancellationToken).ConfigureAwait(false);
		return Ok();
	}

	[HttpPost("retry")]
	public async Task<IActionResult> RetryExecution(
		[FromBody] RetryExecutionCommandDto commandDto,
		CancellationToken cancellationToken)
	{
		if (!await _adminAuthorization.UserIsAdmin().ConfigureAwait(false))
		{
			return Problem(
				title: _languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.Common.Forbidden(),
				statusCode: StatusCodes.Status403Forbidden,
				type: "Forbidden");
		}

		if (!ExecutionSources.RetryExecutionValidExecutionSources.Contains(commandDto.ExecutionSource))
		{
			return Problem(
				title: _languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.DataFlowExecution
					.ExecutionSourceIsNotValid(),
				statusCode: StatusCodes.Status400BadRequest,
				type: "Bad Request");
		}

		await _executionRetrier.RetryExecutionAsync(commandDto, cancellationToken).ConfigureAwait(false);
		return Ok();
	}
}
