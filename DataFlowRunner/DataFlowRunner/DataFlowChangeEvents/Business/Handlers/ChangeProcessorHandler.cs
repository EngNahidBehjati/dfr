using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;

namespace DataFlowRunner.DataFlowChangeEvents.Business.Handlers;

internal sealed class ChangeProcessorHandler : IDataFlowChangeEventHandler<ChangeProcessor>
{
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly IDataFlowExecutionUserIdProvider _dataFlowExecutionUserIdProvider;

	public ChangeProcessorHandler(IServiceScopeFactory serviceScopeFactory,
		IDataFlowExecutionUserIdProvider dataFlowExecutionUserIdProvider)
	{
		_serviceScopeFactory = serviceScopeFactory;
		_dataFlowExecutionUserIdProvider = dataFlowExecutionUserIdProvider;
	}

	public async Task HandleAsync(DataFlowChangeContext context, ChangeProcessor @event,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(@event);

		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IDataFlowExecutionCommandPublisher commandPublisher =
			scope.ServiceProvider.GetRequiredService<IDataFlowExecutionCommandPublisher>();

		ProcessorInfo processorInfo = CreateProcessorInfo(@event.DataFlowProcessorInfo);
		context.ProcessorInfo = processorInfo;

		ChangeDataFlowProcessorCommand changeProcessorCommand =
			CreateChangeProcessorCommand(context.DataFlowExecutionId, processorInfo);

		await commandPublisher.PublishCommandAsync(changeProcessorCommand, cancellationToken)
			.ConfigureAwait(false);
	}

	private ChangeDataFlowProcessorCommand CreateChangeProcessorCommand(DataFlowExecutionId dataFlowExecutionId,
		ProcessorInfo processorInfo)
	{
		UserId userId = _dataFlowExecutionUserIdProvider.GetDataFlowExecutionUserId(dataFlowExecutionId);

		return new ChangeDataFlowProcessorCommand
		{
			GraphExecutionId = dataFlowExecutionId.Value,
			ProcessorInfo = processorInfo,
			UserId = userId.Value
		};
	}

	private static ProcessorInfo CreateProcessorInfo(DataFlowProcessorInfo dataFlowProcessorInfo)
	{
		return new ProcessorInfo
		{
			ProcessorType = dataFlowProcessorInfo.Type,
			ProcessorId = dataFlowProcessorInfo.ProcessorId
		};
	}
}
