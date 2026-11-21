using DataFlowRunner.DataFlowExecutionEvents.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Abstractions;
using Wolverine;

namespace DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration;

internal sealed class WolverineDataFlowExecutionCommandPublisher : IDataFlowExecutionCommandPublisher
{
	private readonly IMessageBus _messageBus;
	private readonly ILogger<WolverineDataFlowExecutionCommandPublisher> _logger;
	private readonly IWolverineExecutionCommandMapper _executionCommandMapper;

	public WolverineDataFlowExecutionCommandPublisher(
		IMessageBus messageBus,
		ILogger<WolverineDataFlowExecutionCommandPublisher> logger,
		IWolverineExecutionCommandMapper executionCommandMapper)
	{
		ArgumentNullException.ThrowIfNull(messageBus);
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(executionCommandMapper);
		_messageBus = messageBus;
		_logger = logger;
		_executionCommandMapper = executionCommandMapper;
	}

	public async ValueTask PublishCommandAsync(IExecutionCommand command, CancellationToken cancellationToken)
	{
		var deliveryOptions = new DeliveryOptions
		{
			PartitionKey = command.GraphExecutionId.ToString()
		};
		using IDisposable? scope = _logger.BeginScope(command.GetLogParameters());
		try
		{
			IWolverineExecutionCommand wolverineCommand = _executionCommandMapper.ToWolverineCommand(command);
			await _messageBus.SendAsync(wolverineCommand, deliveryOptions).ConfigureAwait(false);
			_logger.LogInformation("PublishCommandAsync succeeded");
		}
		catch (Exception e)
		{
			_logger.LogError(e, "PublishCommandAsync failed");
			throw;
		}
	}
}
