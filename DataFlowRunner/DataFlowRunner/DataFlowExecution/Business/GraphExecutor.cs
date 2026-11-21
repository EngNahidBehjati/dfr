using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Exceptions.Conflict;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;
using DataFlowRunner.Language.LanguageDictionary;

namespace DataFlowRunner.DataFlowExecution.Business;

internal sealed class GraphExecutor : IGraphExecutor
{
	private readonly IGraphExecutionSetupService _graphExecutionSetupService;
	private readonly IGraphExecutionEligibilityChecker _graphExecutionEligibilityChecker;
	private readonly INodeExecutor _nodeExecutor;
	private readonly IStartingNodesFinder _startingNodesFinder;
	private readonly INodeExecutionReadinessChecker _nodeExecutionReadinessChecker;
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public GraphExecutor(IGraphExecutionSetupService graphExecutionSetupService,
		IGraphExecutionEligibilityChecker graphExecutionEligibilityChecker,
		INodeExecutor nodeExecutor,
		IStartingNodesFinder startingNodesFinder,
		INodeExecutionReadinessChecker nodeExecutionReadinessChecker,
		IDataFlowExecutionContextProvider dataFlowExecutionContextProvider,
		ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_graphExecutionSetupService = graphExecutionSetupService ??
									  throw new ArgumentNullException(nameof(graphExecutionSetupService));
		_graphExecutionEligibilityChecker = graphExecutionEligibilityChecker ??
											throw new ArgumentNullException(nameof(graphExecutionEligibilityChecker));
		_nodeExecutor = nodeExecutor ?? throw new ArgumentNullException(nameof(nodeExecutor));
		_startingNodesFinder = startingNodesFinder ?? throw new ArgumentNullException(nameof(startingNodesFinder));
		_nodeExecutionReadinessChecker = nodeExecutionReadinessChecker ?? throw new ArgumentNullException(nameof(nodeExecutionReadinessChecker));
		_dataFlowExecutionContextProvider = dataFlowExecutionContextProvider ?? throw new ArgumentNullException(nameof(dataFlowExecutionContextProvider));
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public async Task RunGraphAsync(IReadOnlyGraph graph, DataFlowExecutionId dataFlowExecutionId,
		CancellationToken cancellationToken)
	{
		if (!IsGraphExecutable(graph, dataFlowExecutionId))
		{
			var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage
				.DataFlowRunner.DataFlowExecution.DataFlowNotExecutable();
			throw new DataFlowNotExecutableException(errorMessage);
		}

		await _graphExecutionSetupService
			.SetupExecution(graph, dataFlowExecutionId, cancellationToken).ConfigureAwait(false);

		DataFlowExecutionContext dataFlowExecutionContext = await _dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(dataFlowExecutionId, cancellationToken).ConfigureAwait(false);

		IReadOnlySet<NodeId> startingNodes = _startingNodesFinder.FindStartingNodes(graph)
			.Where(id => _nodeExecutionReadinessChecker.IsNodeReadyToExecute(dataFlowExecutionContext, id))
			.ToHashSet();
		await _nodeExecutor
			.ExecuteNodesAsync(dataFlowExecutionId, startingNodes, cancellationToken).ConfigureAwait(false);
	}

	private bool IsGraphExecutable(IReadOnlyGraph graph, DataFlowExecutionId dataFlowExecutionId)
	{
		return _graphExecutionEligibilityChecker.IsGraphExecutable(graph, dataFlowExecutionId);
	}
}
