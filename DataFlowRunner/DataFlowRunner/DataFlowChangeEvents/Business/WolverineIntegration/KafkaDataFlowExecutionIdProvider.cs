using DataFlowRunner.DataFlowChangeEvents.Abstractions;
using Wolverine;

namespace DataFlowRunner.DataFlowChangeEvents.Business.WolverineIntegration;

internal sealed class KafkaDataFlowExecutionIdProvider : IDataFlowExecutionIdProvider
{
	public bool TryGetDataFlowExecutionId(Envelope envelope, out Guid dataFlowId)
	{
		ArgumentNullException.ThrowIfNull(envelope);

		var key = envelope.PartitionKey;
		return Guid.TryParse(key, out dataFlowId);
	}
}
