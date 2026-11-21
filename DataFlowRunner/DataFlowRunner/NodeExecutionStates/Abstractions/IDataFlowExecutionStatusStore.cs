using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.NodeExecutionStates.Abstractions;

internal interface IDataFlowExecutionStatusStore
{
	DataFlowExecutionStatus Get(DataFlowExecutionId executionId);

	void Set(DataFlowExecutionId executionId, DataFlowExecutionStatus state);
}
