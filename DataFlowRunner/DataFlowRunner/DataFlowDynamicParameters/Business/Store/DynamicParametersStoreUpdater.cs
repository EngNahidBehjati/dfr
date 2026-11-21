using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowDynamicParameters.Business.Store;

internal sealed class DynamicParametersStoreUpdater : IDynamicParametersStoreUpdater
{
	private readonly IDataFlowDynamicParametersStore _dataFlowDynamicParametersStore;
	private readonly IDataFlowExecutionSequenceStore _dataFlowExecutionSequenceStore;

	public DynamicParametersStoreUpdater(
		IDataFlowDynamicParametersStore dataFlowDynamicParametersStore,
		IDataFlowExecutionSequenceStore dataFlowExecutionSequenceStore)
	{
		_dataFlowDynamicParametersStore = dataFlowDynamicParametersStore
		                                  ?? throw new ArgumentNullException(nameof(dataFlowDynamicParametersStore));
		_dataFlowExecutionSequenceStore = dataFlowExecutionSequenceStore
		                        ?? throw new ArgumentNullException(nameof(dataFlowExecutionSequenceStore));
	}

	public void UpdateDataFlowDynamicParameters(
		DataFlowExecutionId executionId,
		IReadOnlyDictionary<NodeId, List<DynamicParameter>> parametersByNode)
	{
		foreach ((NodeId nodeId, List<DynamicParameter> parameters) in parametersByNode)
		{
			_dataFlowDynamicParametersStore.SetDynamicParameters(executionId, [nodeId], parameters);
		}

		_dataFlowExecutionSequenceStore.SetNextRunSequence(executionId, parametersByNode.Keys.ToList());
	}
}
