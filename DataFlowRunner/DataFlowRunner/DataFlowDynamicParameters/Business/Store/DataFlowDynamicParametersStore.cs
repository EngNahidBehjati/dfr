using System.Collections.Concurrent;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;
using DataFlowRunner.DataFlowDynamicParameters.Exceptions;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;

namespace DataFlowRunner.DataFlowDynamicParameters.Business.Store;

internal sealed class DataFlowDynamicParametersStore : IDataFlowDynamicParametersStore
{
	private readonly ConcurrentDictionary<DataFlowExecutionId, INodeDynamicParametersStore> _dynamicParameters = new();
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public DataFlowDynamicParametersStore(ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_languageDictionaryProvider = languageDictionaryProvider ??
		                              throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public List<DynamicParameter> GetDynamicParameters(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId)
	{
		if (_dynamicParameters.TryGetValue(dataFlowExecutionId,
			    out INodeDynamicParametersStore? nodeDynamicParametersContainer))
		{
			return nodeDynamicParametersContainer.GetDynamicParameters(nodeId);
		}

		var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage
			.DataFlowRunner.DynamicParameters.DynamicParametersNotSet(dataFlowExecutionId);
		throw new DynamicParametersNotSetException(errorMessage);
	}

	public bool TryGetDynamicParameters(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId,
		out IReadOnlyCollection<DynamicParameter> parameters)
	{
		if (!_dynamicParameters.TryGetValue(dataFlowExecutionId, out INodeDynamicParametersStore? nodeStore))
		{
			parameters = [];
			return false;
		}

		parameters = nodeStore.GetDynamicParameters(nodeId);
		return true;
	}

	public void SetDynamicParameters(DataFlowExecutionId dataFlowExecutionId,
		IReadOnlyCollection<NodeId> nodeIds,
		List<DynamicParameter> dynamicParameters)
	{
		INodeDynamicParametersStore nodeDynamicParametersStore = _dynamicParameters
			.GetOrAdd(dataFlowExecutionId, static _ => new NodeDynamicParametersStore());

		foreach (NodeId nodeId in nodeIds)
			nodeDynamicParametersStore.SetDynamicParameters(nodeId, dynamicParameters);
	}
}
