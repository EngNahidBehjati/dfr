using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class NodeExecutorEngineGraphInitializer : INodeExecutorEngineGraphInitializer
{
	private readonly IProcessorInfoProvider _processorInfoProvider;
	private readonly INodeExecutorEngineGraphInitializationTracker _graphInitializationTracker;
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly IDataFlowExecutionUserIdProvider _dataFlowExecutionUserIdProvider;

	public NodeExecutorEngineGraphInitializer(IProcessorInfoProvider processorInfoProvider,
		INodeExecutorEngineGraphInitializationTracker nodeExecutorEngineGraphInitializationTracker,
		IServiceScopeFactory serviceScopeFactory,
		IDataFlowExecutionUserIdProvider dataFlowExecutionUserIdProvider)
	{
		_processorInfoProvider = processorInfoProvider ?? throw new ArgumentNullException(nameof(processorInfoProvider));
		_graphInitializationTracker = nodeExecutorEngineGraphInitializationTracker ?? throw new ArgumentNullException(nameof(nodeExecutorEngineGraphInitializationTracker));
		_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		_dataFlowExecutionUserIdProvider = dataFlowExecutionUserIdProvider ?? throw new ArgumentNullException(nameof(dataFlowExecutionUserIdProvider));
	}

	public async Task TryInitialize(DataFlowExecutionId dataFlowExecutionId,
		CancellationToken cancellationToken = default)
	{
		if (_graphInitializationTracker.IsDataFlowInitialized(dataFlowExecutionId))
		{
			return;
		}

		InitializeDataFlowExecutionCommand initializationCommand =
			await CreateInitializationCommandAsync(dataFlowExecutionId, cancellationToken).ConfigureAwait(false);

		await PublishCommandAsync(initializationCommand, cancellationToken).ConfigureAwait(false);
	}

	private async Task<InitializeDataFlowExecutionCommand> CreateInitializationCommandAsync(
		DataFlowExecutionId dataFlowExecutionId, CancellationToken cancellationToken)
	{
		ProcessorInfo processorInfo = await _processorInfoProvider
			.GetProcessorInfoAsync(dataFlowExecutionId, cancellationToken).ConfigureAwait(false);
		UserId userId = _dataFlowExecutionUserIdProvider.GetDataFlowExecutionUserId(dataFlowExecutionId);
		return new InitializeDataFlowExecutionCommand
		{
			DataFlowId = dataFlowExecutionId.Value,
			GraphExecutionId = dataFlowExecutionId.Value,
			UserId = userId.Value,
			ProcessorInfo = processorInfo
		};
	}

	private async Task PublishCommandAsync(
		InitializeDataFlowExecutionCommand command, CancellationToken cancellationToken)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IDataFlowExecutionCommandPublisher commandPublisher =
			scope.ServiceProvider.GetRequiredService<IDataFlowExecutionCommandPublisher>();

		await commandPublisher.PublishCommandAsync(command, cancellationToken).ConfigureAwait(false);
	}
}
