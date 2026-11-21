using System.Diagnostics.CodeAnalysis;
using DataFlowRunner.Common.Exceptions.NotFound;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowSnapshot.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Abstractions;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class DataFlowExecutionContextProvider : IDataFlowExecutionContextProvider
{
	private readonly IDataFlowExecutionContextContainer _dataFlowExecutionContextContainer;
	private readonly IDataFlowExecutionContextCreator _dataFlowExecutionContextCreator;
	private readonly INodeExecutionStateUpdater _nodeExecutionStateUpdater;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public DataFlowExecutionContextProvider(IDataFlowExecutionContextContainer dataFlowExecutionContextContainer,
		IDataFlowExecutionContextCreator dataFlowExecutionContextCreator,
		INodeExecutionStateUpdater nodeExecutionStateUpdater,
		ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_dataFlowExecutionContextContainer = dataFlowExecutionContextContainer ?? throw new ArgumentNullException(nameof(dataFlowExecutionContextContainer));
		_dataFlowExecutionContextCreator = dataFlowExecutionContextCreator ?? throw new ArgumentNullException(nameof(dataFlowExecutionContextCreator));
		_nodeExecutionStateUpdater = nodeExecutionStateUpdater ?? throw new ArgumentNullException(nameof(nodeExecutionStateUpdater));
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public async Task UpdateDataFlowExecutionContextAsync(DataFlowExecutionId dataFlowExecutionId,
		CancellationToken cancellationToken,
		DataFlowVersion? dataFlowVersion = null)
	{
		DataFlowExecutionContext dataFlowExecutionContext = await _dataFlowExecutionContextCreator
			.CreateDataFlowExecutionContextAsync(dataFlowExecutionId.Value, cancellationToken, dataFlowVersion?.Value)
			.ConfigureAwait(false);

		TryAddDataFlowNodeExecutionStates(dataFlowExecutionId, dataFlowExecutionContext);

		_dataFlowExecutionContextContainer.AddOrUpdateDataFlowExecutionContext(dataFlowExecutionContext);
	}

	public Task<DataFlowExecutionContext> GetRunningDataFlowExecutionContextAsync(DataFlowExecutionId dataFlowExecutionId,
		CancellationToken cancellationToken)
	{
		if (_dataFlowExecutionContextContainer.TryGetDataFlowExecutionContext(dataFlowExecutionId,
			    out DataFlowExecutionContext? dataFlowExecutionContext))
		{
			return Task.FromResult(dataFlowExecutionContext);
		}

		var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage
			.DataFlowRunner.DataFlowExecution.DataFlowExecutionNotFound();
		throw new DataFlowExecutionNotFoundException(errorMessage);
	}

	public bool TryGetRunningDataFlowExecutionContext(DataFlowExecutionId dataFlowExecutionId,
		[NotNullWhen(true)] out DataFlowExecutionContext? dataFlowExecutionContext)
	{
		return _dataFlowExecutionContextContainer
			.TryGetDataFlowExecutionContext(dataFlowExecutionId, out dataFlowExecutionContext);
	}

	private void TryAddDataFlowNodeExecutionStates(DataFlowExecutionId dataFlowExecutionId,
		DataFlowExecutionContext dataFlowExecutionContext)
	{
		IReadOnlyCollection<Node> allNodes = dataFlowExecutionContext.Graph.GetAllNodes();
		foreach (Node node in allNodes)
		{
			_nodeExecutionStateUpdater.TryAddNodeExecutionState(dataFlowExecutionId, node.Id);
		}
	}
}
