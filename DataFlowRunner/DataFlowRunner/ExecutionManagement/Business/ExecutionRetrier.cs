using DataFlowRunner.Common.Authentication.Abstractions;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.ExecutionManagement.Abstractions;
using DataFlowRunner.ExecutionManagement.Exceptions;
using DataFlowRunner.ExecutionManagement.Models;
using DataFlowRunner.Language.LanguageDictionary;

namespace DataFlowRunner.ExecutionManagement.Business;

internal sealed class ExecutionRetrier : IExecutionRetrier
{
	private readonly IRetryExecutionCommandInvoker _retryExecutionCommandInvoker;
	private readonly IAuthenticatedUserProvider _authenticatedUserProvider;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;
	private readonly ILogger<ExecutionRetrier> _logger;

	public ExecutionRetrier(
		IRetryExecutionCommandInvoker retryExecutionCommandInvoker,
		IAuthenticatedUserProvider authenticatedUserProvider,
		ILanguageDictionaryProvider languageDictionaryProvider,
		ILogger<ExecutionRetrier> logger)
	{
		_retryExecutionCommandInvoker = retryExecutionCommandInvoker ?? throw new ArgumentNullException(nameof(retryExecutionCommandInvoker));
		_authenticatedUserProvider = authenticatedUserProvider ?? throw new ArgumentNullException(nameof(authenticatedUserProvider));
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task RetryExecutionAsync(RetryExecutionCommandDto commandDto, CancellationToken cancellationToken)
	{
		UserId userId = await _authenticatedUserProvider.GetCurrentUserId(cancellationToken).ConfigureAwait(false);

		_logger.LogInformation(
			"Retry requested from source {ExecutionSource} for dataFlow with id {DataFlowId} and execution id {ExecutionId} by user with id {UserId}",
			commandDto.ExecutionSource, commandDto.DataFlowId, commandDto.DataFlowExecutionId, userId);

		RetryExecutionCommand retryExecutionCommand = new()
		{
			ExecutionSource = commandDto.ExecutionSource,
			DataFlowExecutionId = commandDto.DataFlowExecutionId,
			DataFlowId = commandDto.DataFlowId,
			UserId = userId.Value,
			DynamicParameters = commandDto.DynamicParameters
		};

		RetryExecutionReport report = await
			InvokeRetryExecution(retryExecutionCommand, cancellationToken).ConfigureAwait(false);

		if (!report.IsSucceeded)
		{
			var message = _languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.DataFlowExecution
				.RetryExecutionFailed();
			if (!string.IsNullOrWhiteSpace(report.ErrorMessage))
			{
				message += "\n\t- " + report.ErrorMessage;
			}

			throw new RetryExecutionFailedException(message);
		}
	}

	private async Task<RetryExecutionReport> InvokeRetryExecution(RetryExecutionCommand retryExecutionCommand,
		CancellationToken cancellationToken)
	{
		try
		{
			return await _retryExecutionCommandInvoker.InvokeAsync(retryExecutionCommand, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception)
		{
			throw new RetryExecutionFailedException(
				_languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.DataFlowExecution.RetryExecutionTimedOut());
		}
	}
}
