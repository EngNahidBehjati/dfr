using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;
using DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;
using DataFlowRunner.DataFlowSynchronization.Abstraction;
using DataFlowRunner.DataFlowSynchronization.Constants;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;

namespace DataFlowRunner.DataFlowExecutionReportHandlers.Handlers;

internal sealed class GraphInitializationNotRequestedReportHandler : IGraphInitializationNotRequestedReportHandler
{
	private readonly INodeExecutionStateUpdater _nodeExecutionStateUpdater;
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly INodeExecutorEngineGraphInitializationTracker _nodeExecutorEngineGraphInitializationTracker;
	private readonly IDataFlowExecutionStatusStore _dataFlowExecutionStatusStore;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;
	private readonly IDataFlowActionLockExecutor _actionLockExecutor;

	public GraphInitializationNotRequestedReportHandler(INodeExecutionStateUpdater nodeExecutionStateUpdater,
		IDataFlowExecutionContextProvider dataFlowExecutionContextProvider,
		INodeExecutorEngineGraphInitializationTracker nodeExecutorEngineGraphInitializationTracker,
		IDataFlowExecutionStatusStore dataFlowExecutionStatusStore,
		ILanguageDictionaryProvider languageDictionaryProvider,
		IDataFlowActionLockExecutor actionLockExecutor)
	{
		_nodeExecutionStateUpdater = nodeExecutionStateUpdater ?? throw new ArgumentNullException(nameof(nodeExecutionStateUpdater));
		_dataFlowExecutionContextProvider = dataFlowExecutionContextProvider ?? throw new ArgumentNullException(nameof(dataFlowExecutionContextProvider));
		_nodeExecutorEngineGraphInitializationTracker = nodeExecutorEngineGraphInitializationTracker ?? throw new ArgumentNullException(nameof(nodeExecutorEngineGraphInitializationTracker));
		_dataFlowExecutionStatusStore = dataFlowExecutionStatusStore ?? throw new ArgumentNullException(nameof(dataFlowExecutionStatusStore));
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
		_actionLockExecutor = actionLockExecutor ?? throw new ArgumentNullException(nameof(actionLockExecutor));
	}

	public async Task HandleAsync(GraphInitializationNotRequestedReport report, CancellationToken cancellationToken)
	{
		var dataFlowExecutionId = new DataFlowExecutionId(report.GraphExecutionId);

		Func<Task> action = async () =>
		{
			DataFlowExecutionContext dataFlowExecutionContext = await _dataFlowExecutionContextProvider
				.GetRunningDataFlowExecutionContextAsync(dataFlowExecutionId, cancellationToken).ConfigureAwait(false);

			foreach (Node node in dataFlowExecutionContext.Graph.GetAllNodes())
			{
				_nodeExecutionStateUpdater.ResetNodeExecutionState(dataFlowExecutionId, node.Id);
			}
		};
		await _actionLockExecutor.InvokeAsync(
			dataFlowExecutionId, LockActionKeys.DataFlowExecution, action, cancellationToken).ConfigureAwait(false);

		_nodeExecutorEngineGraphInitializationTracker.UntrackDataFlowInitialization(dataFlowExecutionId);
		var dataFlowExecutionStatus = new DataFlowExecutionStatus(
			_languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner.DataFlowExecutionEvents.ExecutorReset());
		_dataFlowExecutionStatusStore.Set(dataFlowExecutionId, dataFlowExecutionStatus);
	}
}
