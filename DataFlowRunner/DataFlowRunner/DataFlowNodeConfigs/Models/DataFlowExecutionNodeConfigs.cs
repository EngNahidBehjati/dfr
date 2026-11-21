using System.Collections.Concurrent;
using DataFlowRunner.Common.Exceptions.NotFound;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;

namespace DataFlowRunner.DataFlowNodeConfigs.Models;

internal sealed class DataFlowExecutionNodeConfigs
{
	private readonly ConcurrentDictionary<NodeId, NodeConfig> _configs = new();
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public DataFlowExecutionNodeConfigs(ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_languageDictionaryProvider = languageDictionaryProvider ??
		                              throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public NodeConfig GetConfig(NodeId nodeId)
	{
		if (_configs.TryGetValue(nodeId, out NodeConfig? config))
		{
			return config;
		}

		var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage
			.DataFlowRunner.GraphCore.NodeNotFound();

		throw new NodeNotFoundException(errorMessage);
	}

	public void SetConfig(NodeId nodeId, NodeConfig config)
	{
		_configs[nodeId] = config;
	}
}
