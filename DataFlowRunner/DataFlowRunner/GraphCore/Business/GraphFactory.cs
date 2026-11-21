using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models;
using DataFlowRunner.Language.LanguageDictionary;

namespace DataFlowRunner.GraphCore.Business;

internal class GraphFactory : IGraphFactory
{
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public GraphFactory(ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public IGraph CreateGraph() => new Graph(_languageDictionaryProvider);
}
