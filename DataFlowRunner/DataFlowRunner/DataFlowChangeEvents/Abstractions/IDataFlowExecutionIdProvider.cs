using Wolverine;

namespace DataFlowRunner.DataFlowChangeEvents.Abstractions;

internal interface IDataFlowExecutionIdProvider
{
	bool TryGetDataFlowExecutionId(Envelope envelope, out Guid dataFlowId);
}
