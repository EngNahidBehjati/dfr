using System.Collections.Concurrent;
using DataFlowRunner.Common.Exceptions.NotFound;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodePortConnections.Abstract;
using DataFlowRunner.NodePortConnections.Models;

namespace DataFlowRunner.NodePortConnections.Business;

internal sealed class EdgeByPortContainer : IEdgeByPortContainer
{
	private readonly ConcurrentDictionary<EdgeId, EdgeByPort> _edgeByIds = new();
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public EdgeByPortContainer(ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public EdgeByPort GetEdgeByPort(EdgeId edgeId)
	{
		if (_edgeByIds.TryGetValue(edgeId, out EdgeByPort? edgeByPort))
		{
			return edgeByPort;
		}

		var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage
			.DataFlowRunner.GraphCore.EdgeNotFound();
		throw new EdgeNotFoundException(errorMessage);
	}

	public bool TryAddEdgeByPort(EdgeByPort edgeByPort)
	{
		return _edgeByIds.TryAdd(edgeByPort.Id, edgeByPort);
	}
}
