using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class NodeCanceller : INodeCanceller
{
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly IDataFlowExecutionUserIdProvider _dataFlowExecutionUserIdProvider;

	public NodeCanceller(IServiceScopeFactory serviceScopeFactory, IDataFlowExecutionUserIdProvider dataFlowExecutionUserIdProvider)
	{
		_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		_dataFlowExecutionUserIdProvider = dataFlowExecutionUserIdProvider ?? throw new ArgumentNullException(nameof(dataFlowExecutionUserIdProvider));
	}

	public async Task CancelNodeAsync(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId,
		CancellationToken cancellationToken)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IDataFlowExecutionCommandPublisher commandPublisher =
			scope.ServiceProvider.GetRequiredService<IDataFlowExecutionCommandPublisher>();
		UserId userId = _dataFlowExecutionUserIdProvider.GetDataFlowExecutionUserId(dataFlowExecutionId);
		var cancelNodeCommand = new CancelNodeCommand
		{
			GraphExecutionId = dataFlowExecutionId.Value,
			NodeId = nodeId.Value,
			UserId = userId.Value
		};
		await commandPublisher.PublishCommandAsync(cancelNodeCommand, cancellationToken)
			.ConfigureAwait(false);
	}
}
