using DataFlowRunner.ExecutionManagement.Abstractions;
using DataFlowRunner.ExecutionManagement.Models;
using DataFlowRunner.ExecutionManagement.Options;
using Microsoft.Extensions.Options;
using Wolverine;

namespace DataFlowRunner.ExecutionManagement.WolverineIntegration;

internal sealed class WolverineRetryExecutionCommandInvoker : IRetryExecutionCommandInvoker
{
	private readonly TimeSpan _invokeTimeout;
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<WolverineRetryExecutionCommandInvoker> _logger;

	public WolverineRetryExecutionCommandInvoker(
		IServiceScopeFactory serviceScopeFactory,
		IOptions<RetryExecutionInvokeTimeoutOptions> invokeTimeoutOptions,
		ILogger<WolverineRetryExecutionCommandInvoker> logger)
	{
		_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		ArgumentNullException.ThrowIfNull(invokeTimeoutOptions);
		_invokeTimeout = invokeTimeoutOptions.Value.TimeoutInSeconds;
	}

	public async Task<RetryExecutionReport> InvokeAsync(RetryExecutionCommand command, CancellationToken cancellationToken)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IMessageBus messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

		using IDisposable? disposable = _logger.BeginScope(new Dictionary<string, object>
		{
			["ExecutionSource"] = command.ExecutionSource,
			["DataFlowId"] = command.DataFlowId,
			["DataFlowExecutionId"] = command.DataFlowExecutionId,
			["UserId"] = command.UserId
		});

		_logger.LogInformation("ExecutionManagement retryCommand invoke started");
		try
		{
			RetryExecutionReport report = await messageBus
				.InvokeAsync<RetryExecutionReport>(command, cancellationToken, _invokeTimeout)
				.ConfigureAwait(false);
			_logger.LogInformation("ExecutionManagement retryCommand invoke completed");
			return report;
		}
		catch (Exception e)
		{
			_logger.LogError(e, "ExecutionManagement retryCommand invoke failed");
			throw;
		}
	}
}
