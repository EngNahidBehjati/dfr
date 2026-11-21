using System.Collections.Concurrent;
using DataFlowRunner.Common.Exceptions.NotFound;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowNodeConfigs.Abstractions;
using DataFlowRunner.DataFlowNodeConfigs.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;

namespace DataFlowRunner.DataFlowNodeConfigs.Business;

internal sealed class InMemoryDataFlowNodeConfigsStore : IDataFlowNodeConfigsStore
{
	private readonly ConcurrentDictionary<DataFlowExecutionId, DataFlowExecutionNodeConfigs> _dataFlowConfigs = new();
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public InMemoryDataFlowNodeConfigsStore(ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public NodeConfig GetNodeConfig(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId)
	{
		if (!_dataFlowConfigs.TryGetValue(dataFlowExecutionId,
				out DataFlowExecutionNodeConfigs? dataFlowExecutionConfig))
		{
			var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage
				.DataFlowRunner.DataFlowExecution.DataFlowExecutionNotFound();

			throw new DataFlowExecutionNotFoundException(errorMessage);
		}

		return dataFlowExecutionConfig.GetConfig(nodeId);
	}
	public void SetNodeConfig(DataFlowExecutionId dataFlowExecutionId, NodeId nodeId, NodeConfig nodeConfig)
	{
		GetDataFlowExecutionNodeConfigs(dataFlowExecutionId).SetConfig(nodeId, nodeConfig);
	}

	private DataFlowExecutionNodeConfigs GetDataFlowExecutionNodeConfigs(DataFlowExecutionId dataFlowExecutionId)
	{
		return _dataFlowConfigs.GetOrAdd(
			dataFlowExecutionId,
			_ => new DataFlowExecutionNodeConfigs(_languageDictionaryProvider)
		);
	}
}
