using DataFlowRunner.Common.Configuration;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Exceptions.Conflict;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.Language.LanguageDictionary;
using Microsoft.Extensions.Options;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class DataFlowVersionConflictHandler : IDataFlowVersionConflictHandler
{
	private static readonly TimeSpan _waitTimeout = TimeSpan.FromSeconds(2);
	private static readonly TimeSpan _pollInterval = TimeSpan.FromMilliseconds(20);

	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;
	private readonly IDataFlowInvalidator _dataFlowInvalidator;
	private readonly IOptions<DataFlowExecutionFeatureFlags> _featureFlagsOptions;

	public DataFlowVersionConflictHandler(IDataFlowExecutionContextProvider dataFlowExecutionContextProvider,
		ILanguageDictionaryProvider languageDictionaryProvider,
		IDataFlowInvalidator dataFlowInvalidator,
		IOptions<DataFlowExecutionFeatureFlags> featureFlagsOptions)
	{
		_dataFlowExecutionContextProvider = dataFlowExecutionContextProvider ?? throw new ArgumentNullException(nameof(dataFlowExecutionContextProvider));
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
		_dataFlowInvalidator = dataFlowInvalidator ?? throw new ArgumentNullException(nameof(dataFlowInvalidator));
		_featureFlagsOptions = featureFlagsOptions ?? throw new ArgumentNullException(nameof(featureFlagsOptions));
	}

	public async Task HandleVersionConflictAsync(DataFlowExecutionId dataFlowExecutionId,
		DataFlowVersion dataFlowVersion,
		CancellationToken cancellationToken)
	{
		if (!_dataFlowExecutionContextProvider.TryGetRunningDataFlowExecutionContext(
			    dataFlowExecutionId, out DataFlowExecutionContext? runningContext))
		{
			await _dataFlowExecutionContextProvider
				.UpdateDataFlowExecutionContextAsync(dataFlowExecutionId, cancellationToken, dataFlowVersion)
				.ConfigureAwait(false);
			return;
		}

		if (runningContext.DataFlowVersion.Value > dataFlowVersion.Value)
		{
			throw CreateProvidedVersionIsOutdatedException();
		}

		if (runningContext.DataFlowVersion.Value == dataFlowVersion.Value)
		{
			return;
		}

		if (_featureFlagsOptions.Value.WaitForDataFlowVersionUpdate)
		{
			await WaitForTargetVersionAsync(dataFlowExecutionId, dataFlowVersion, cancellationToken).ConfigureAwait(false);
		}
		else
		{
			await _dataFlowInvalidator.InvalidateDataFlowAsync(runningContext, cancellationToken)
				.ConfigureAwait(false);
			await _dataFlowExecutionContextProvider
				.UpdateDataFlowExecutionContextAsync(dataFlowExecutionId, cancellationToken, dataFlowVersion)
				.ConfigureAwait(false);
		}
	}

	private async Task WaitForTargetVersionAsync(DataFlowExecutionId dataFlowExecutionId,
		DataFlowVersion expectedVersion,
		CancellationToken cancellationToken)
	{
		using var timeoutCancellationTokenSource = new CancellationTokenSource(_waitTimeout);
		using var linkedCancellationTokenSource =
			CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellationTokenSource.Token);

		try
		{
			while (true)
			{
				linkedCancellationTokenSource.Token.ThrowIfCancellationRequested();

				DataFlowExecutionContext runningContext = await _dataFlowExecutionContextProvider
					.GetRunningDataFlowExecutionContextAsync(dataFlowExecutionId, linkedCancellationTokenSource.Token)
					.ConfigureAwait(false);
				var runningVersion = runningContext.DataFlowVersion.Value;
				if (runningVersion == expectedVersion.Value)
				{
					return;
				}

				if (runningVersion > expectedVersion.Value)
				{
					throw CreateProvidedVersionIsOutdatedException();
				}

				await Task.Delay(_pollInterval, linkedCancellationTokenSource.Token).ConfigureAwait(false);
			}
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			throw CreateWaitForVersionTimeoutException();
		}
	}

	private DataFlowVersionConflictException CreateProvidedVersionIsOutdatedException()
	{
		return new DataFlowVersionConflictException(
			_languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.DataFlowExecution.ProvidedVersionIsOutdated());
	}

	private DataFlowVersionConflictException CreateWaitForVersionTimeoutException()
	{
		return new DataFlowVersionConflictException(
			_languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.DataFlowExecution.WaitForVersionTimeout());
	}
}
