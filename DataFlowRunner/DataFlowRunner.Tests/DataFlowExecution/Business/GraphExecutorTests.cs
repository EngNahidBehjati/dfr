using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.DataFlowExecution.Exceptions.Conflict;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.Tests.Common;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecution.Business;

public class GraphExecutorTests
{
	private readonly IGraphExecutionSetupService _graphExecutionSetupService;
	private readonly IGraphExecutionEligibilityChecker _eligibilityChecker;
	private readonly INodeExecutor _nodeExecutor;
	private readonly IStartingNodesFinder _startingNodesFinder;
	private readonly INodeExecutionReadinessChecker _nodeExecutionReadinessChecker;
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly GraphExecutor _sut;

	public GraphExecutorTests()
	{
		_graphExecutionSetupService = Substitute.For<IGraphExecutionSetupService>();
		_eligibilityChecker = Substitute.For<IGraphExecutionEligibilityChecker>();
		_nodeExecutor = Substitute.For<INodeExecutor>();
		_startingNodesFinder = Substitute.For<IStartingNodesFinder>();
		_nodeExecutionReadinessChecker = Substitute.For<INodeExecutionReadinessChecker>();
		_dataFlowExecutionContextProvider  = Substitute.For<IDataFlowExecutionContextProvider>();
		ILanguageDictionaryProvider languageDictionaryProvider = LanguageDictionaryProvider.Instance;
		_sut = new GraphExecutor(_graphExecutionSetupService, _eligibilityChecker, _nodeExecutor, _startingNodesFinder,
			_nodeExecutionReadinessChecker, _dataFlowExecutionContextProvider, languageDictionaryProvider);
	}

	[Fact]
	public async Task RunGraphAsync_ShouldThrowDataFlowNotExecutableException_WhenGraphIsNotExecutable()
	{
		// Arrange
		IReadOnlyGraph? graph = Substitute.For<IReadOnlyGraph>();
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		_eligibilityChecker.IsGraphExecutable(graph, executionId).Returns(false);

		// Act
		Func<Task> act = () => _sut.RunGraphAsync(graph, executionId, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<DataFlowNotExecutableException>();
		await _graphExecutionSetupService.DidNotReceiveWithAnyArgs()
			.SetupExecution(null!, default, CancellationToken.None);
		await _nodeExecutor.DidNotReceiveWithAnyArgs()
			.ExecuteNodesAsync(executionId, null!, CancellationToken.None);
	}

	[Fact]
	public async Task RunGraphAsync_ShouldSetupAndExecute_WhenGraphIsExecutable()
	{
		// Arrange
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();

		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		_eligibilityChecker.IsGraphExecutable(graph, executionId).Returns(true);
		var startingNode1 = NodeId.New();
		var startingNode2 = NodeId.New();
		var startingNodes = new HashSet<NodeId> {startingNode1, startingNode2};
		_startingNodesFinder.FindStartingNodes(graph).Returns(startingNodes);

		var context = new DataFlowExecutionContext()
		{
			DataFlowExecutionId = executionId,
			DataFlowVersion = new DataFlowVersion(),
			Graph = Substitute.For<IGraph>(),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
		_dataFlowExecutionContextProvider.GetRunningDataFlowExecutionContextAsync(executionId, CancellationToken.None)
			.Returns(context);
		_nodeExecutionReadinessChecker.IsNodeReadyToExecute(context, startingNode1).Returns(true);
		_nodeExecutionReadinessChecker.IsNodeReadyToExecute(context, startingNode2).Returns(false);

		// Act
		await _sut.RunGraphAsync(graph, executionId, CancellationToken.None);

		// Assert
		await _graphExecutionSetupService.Received().SetupExecution(graph, executionId, CancellationToken.None);
		_startingNodesFinder.Received().FindStartingNodes(graph);
		await _nodeExecutor.Received().ExecuteNodesAsync(executionId,
			Arg.Is<HashSet<NodeId>>(ids => ids.Count == 1 && ids.Contains(startingNode1)),
			CancellationToken.None);
	}
}
