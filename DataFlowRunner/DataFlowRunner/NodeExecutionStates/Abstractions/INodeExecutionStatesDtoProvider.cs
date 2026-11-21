using DataFlowRunner.NodeExecutionStates.DataTransferObjects;

namespace DataFlowRunner.NodeExecutionStates.Abstractions;

public interface INodeExecutionStatesDtoProvider
{
	Task<DataFlowNodeExecutionStatesDto> GetAllNodeExecutionStatesAsync(Guid dataFlowExecutionId,
		CancellationToken cancellationToken);
}

