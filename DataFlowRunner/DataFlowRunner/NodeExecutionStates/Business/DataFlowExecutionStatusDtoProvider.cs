using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.DataTransferObjects;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Business;

internal sealed class DataFlowExecutionStatusDtoProvider : IDataFlowExecutionStatusDtoProvider
{
	private readonly INodeExecutionStatesDtoProvider _nodeExecutionStatesDtoProvider;
	private readonly IDataFlowExecutionStatusStore _dataFlowExecutionStatusStore;

	public DataFlowExecutionStatusDtoProvider(INodeExecutionStatesDtoProvider nodeExecutionStatesDtoProvider,
		IDataFlowExecutionStatusStore dataFlowExecutionStatusStore)
	{
		_nodeExecutionStatesDtoProvider = nodeExecutionStatesDtoProvider ?? throw new ArgumentNullException(nameof(nodeExecutionStatesDtoProvider));
		_dataFlowExecutionStatusStore = dataFlowExecutionStatusStore ?? throw new ArgumentNullException(nameof(dataFlowExecutionStatusStore));
	}

	public async Task<DataFlowExecutionStatusDto> GetDataFlowExecutionStatusAsync(Guid dataFlowExecutionId, CancellationToken cancellationToken)
	{
		DataFlowNodeExecutionStatesDto nodeExecutionStates = await
			_nodeExecutionStatesDtoProvider.GetAllNodeExecutionStatesAsync(dataFlowExecutionId, cancellationToken)
			.ConfigureAwait(false);

		var executionId = new DataFlowExecutionId(dataFlowExecutionId);
		DataFlowExecutionStatus executionStatus = _dataFlowExecutionStatusStore.Get(executionId);
		return new DataFlowExecutionStatusDto
		{
			ExecutionStatus = new ExecutionStatusDto
			{
				Failed = executionStatus.Failed,
				Message = executionStatus.FailureMessage
			},
			NodeExecutionStates = nodeExecutionStates.NodeExecutionStates
		};
	}
}
