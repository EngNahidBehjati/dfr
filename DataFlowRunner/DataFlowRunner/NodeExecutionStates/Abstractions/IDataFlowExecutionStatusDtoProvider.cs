using DataFlowRunner.NodeExecutionStates.DataTransferObjects;

namespace DataFlowRunner.NodeExecutionStates.Abstractions;

public interface IDataFlowExecutionStatusDtoProvider
{
	Task<DataFlowExecutionStatusDto> GetDataFlowExecutionStatusAsync(Guid dataFlowExecutionId,
		CancellationToken cancellationToken);
}
