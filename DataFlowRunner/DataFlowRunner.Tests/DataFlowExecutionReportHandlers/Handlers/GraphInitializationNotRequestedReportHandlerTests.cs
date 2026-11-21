using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;
using DataFlowRunner.DataFlowExecutionReportHandlers.Handlers;
using DataFlowRunner.DataFlowSynchronization.Abstraction;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;
using DataFlowRunner.Tests.Common;
using DataFlowRunner.Tests.Common.FakeClasses;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecutionReportHandlers.Handlers;

public class GraphInitializationNotRequestedReportHandlerTests
{
	private readonly INodeExecutionStateUpdater _nodeExecutionStateUpdater;
	private readonly IDataFlowExecutionContextProvider _contextProvider;
	private readonly INodeExecutorEngineGraphInitializationTracker _graphInitializationTracker;
	private readonly IDataFlowExecutionStatusStore _dataFlowExecutionStatusStore;
	private readonly GraphInitializationNotRequestedReportHandler _sut;

	public GraphInitializationNotRequestedReportHandlerTests()
	{
		_nodeExecutionStateUpdater = Substitute.For<INodeExecutionStateUpdater>();
		_contextProvider = Substitute.For<IDataFlowExecutionContextProvider>();
		_graphInitializationTracker = Substitute.For<INodeExecutorEngineGraphInitializationTracker>();
		_dataFlowExecutionStatusStore = Substitute.For<IDataFlowExecutionStatusStore>();
		LanguageDictionaryProvider languageDictionaryProvider = LanguageDictionaryProvider.Instance;
		IDataFlowActionLockExecutor actionLockExecutor = new FakeActionLockExecutor();
		_sut = new GraphInitializationNotRequestedReportHandler(_nodeExecutionStateUpdater, _contextProvider,
			_graphInitializationTracker, _dataFlowExecutionStatusStore, languageDictionaryProvider, actionLockExecutor);
	}

	[Fact]
	public async Task HandleAsync_ShouldResetDataFlowExecutionStateAndRecordExecutionStatus_WhenReportIsProvided()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var report = new GraphInitializationNotRequestedReport
		{
			GraphExecutionId = dataFlowExecutionId.Value
		};

		var context = new DataFlowExecutionContext()
		{
			Graph = Substitute.For<IGraph>(),
			DataFlowExecutionId = dataFlowExecutionId,
			DataFlowVersion = new DataFlowVersion(10),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		var nodeId1 = new NodeId(Guid.NewGuid());
		var nodeId2 = new NodeId(Guid.NewGuid());
		var nodes = new List<Node>
		{
			new() { Id = nodeId1 },
			new() { Id = nodeId2 }
		};
		_contextProvider.GetRunningDataFlowExecutionContextAsync(dataFlowExecutionId, CancellationToken.None)
			.Returns(context);
		context.Graph.GetAllNodes().Returns(nodes);

		// Act
		await _sut.HandleAsync(report, CancellationToken.None);

		// Assert
		_nodeExecutionStateUpdater.Received(1).ResetNodeExecutionState(dataFlowExecutionId, nodeId1);
		_nodeExecutionStateUpdater.Received(1).ResetNodeExecutionState(dataFlowExecutionId, nodeId2);
		_graphInitializationTracker.Received(1).UntrackDataFlowInitialization(dataFlowExecutionId);
		_dataFlowExecutionStatusStore.Received(1).Set(dataFlowExecutionId,
			Arg.Is<DataFlowExecutionStatus>(status => status.Failed == true));
	}
}
