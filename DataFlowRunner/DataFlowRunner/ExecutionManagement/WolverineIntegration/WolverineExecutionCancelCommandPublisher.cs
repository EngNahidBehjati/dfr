using DataFlowRunner.ExecutionManagement.Abstractions;
using DataFlowRunner.ExecutionManagement.Models;
using Wolverine;

namespace DataFlowRunner.ExecutionManagement.WolverineIntegration;

internal sealed class WolverineExecutionCancelCommandPublisher : ICancelExecutionCommandPublisher
{
	private readonly IMessageBus _messageBus;
	private readonly ILogger<WolverineExecutionCancelCommandPublisher> _logger;

	public WolverineExecutionCancelCommandPublisher(IMessageBus messageBus,
		ILogger<WolverineExecutionCancelCommandPublisher> logger)
	{
		_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async ValueTask PublishCommandAsync(CancelCommandDto commandDto, CancellationToken cancellationToken)
	{
		using IDisposable? scope = _logger.BeginScope(new Dictionary<string, object>
		{
			["ExecutionSource"] = commandDto.ExecutionSource,
			["DataFlowId"] = commandDto.DataFlowId,
			["DataFlowExecutionId"] = commandDto.DataFlowExecutionId
		});
		try
		{
			CancelExecutionCommand cancelExecutionCommand = new()
			{
				ExecutionSource = commandDto.ExecutionSource,
				DataFlowExecutionId = commandDto.DataFlowExecutionId,
				DataFlowId = commandDto.DataFlowId
			};

			await _messageBus.SendAsync(cancelExecutionCommand).ConfigureAwait(false);
			_logger.LogInformation("ExecutionManagement cancelCommand published");
		}
		catch (Exception e)
		{
			_logger.LogError(e, "ExecutionManagement cancelCommand publish failed");
			throw;
		}
	}
}
