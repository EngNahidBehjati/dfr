using DataFlowRunner.DataFlowDynamicParameters.Abstractions.ProvideRequiredDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowDynamicParameters.Business.ProvideRequiredDynamicParameters;

internal sealed class DataFlowParametersValueExtractor : IDataFlowParametersValueExtractor
{
	private readonly IDataFlowDynamicParametersStore _dataFlowDynamicParametersStore;
	private readonly IDataFlowExecutionSequenceStore _dataFlowExecutionSequenceStore;

	public DataFlowParametersValueExtractor(
		IDataFlowDynamicParametersStore dataFlowDynamicParametersStore,
		IDataFlowExecutionSequenceStore dataFlowExecutionSequenceStore)
	{
		_dataFlowDynamicParametersStore = dataFlowDynamicParametersStore
		                                  ?? throw new ArgumentNullException(nameof(dataFlowDynamicParametersStore));
		_dataFlowExecutionSequenceStore = dataFlowExecutionSequenceStore ??
		                                 throw new ArgumentNullException(nameof(dataFlowExecutionSequenceStore));
	}

	public IReadOnlyCollection<DynamicParameter> Extract(
		DataFlowExecutionId executionId,
		IReadOnlyGraph graph)
	{
		var candidateParameterByName = new Dictionary<string, (DynamicParameter Parameter, long Sequence)>();

		foreach (Node node in graph.GetAllNodes())
		{
			var nodeRunSequence = _dataFlowExecutionSequenceStore.GetNodeRunSequence(executionId, node.Id);

			if (!_dataFlowDynamicParametersStore.TryGetDynamicParameters(executionId, node.Id,
				    out IReadOnlyCollection<DynamicParameter> parameters))
			{
				continue;
			}

			foreach (DynamicParameter parameter in parameters)
			{
				if (HasHigherRunSequenceThanStored(parameter.Name, nodeRunSequence, candidateParameterByName))
				{
					candidateParameterByName[parameter.Name] = (parameter, nodeRunSequence);
				}
			}
		}

		return candidateParameterByName.Values.Select(candidate => candidate.Parameter).ToList();
	}

	private static bool HasHigherRunSequenceThanStored(string parameterName, long sequence,
		Dictionary<string, (DynamicParameter Parameter, long Sequence)> candidateParameterByName)
	{
		var isParameterVisited = candidateParameterByName.TryGetValue(parameterName,
			out (DynamicParameter Parameter, long Sequence) existing);
		return !isParameterVisited || sequence > existing.Sequence;
	}
}
