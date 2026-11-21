using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;
using DataFlowRunner.DataFlowExecutionReportHandlers.Business;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;
using DataFlowRunner.Tests.Common;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecutionReportHandlers.Business;

public sealed class ChildNodesExecutorTests
{
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly IChildNodesFinder _childNodesFinder;
	private readonly INodeExecutionReadinessChecker _nodeExecutionReadinessChecker;
	private readonly INodeExecutor _nodeExecutor;
	private readonly ChildNodesExecutor _sut;

	public ChildNodesExecutorTests()
	{
		_dataFlowExecutionContextProvider = Substitute.For<IDataFlowExecutionContextProvider>();
		_childNodesFinder = Substitute.For<IChildNodesFinder>();
		_nodeExecutionReadinessChecker = Substitute.For<INodeExecutionReadinessChecker>();
		_nodeExecutor = Substitute.For<INodeExecutor>();
		_sut = new ChildNodesExecutor(_dataFlowExecutionContextProvider, _childNodesFinder, _nodeExecutionReadinessChecker, _nodeExecutor);
	}

	[Fact]
	public async Task RunReadyChildNodes_ShouldExecuteReadyChildNodeAndDontExecuteNotReadyChildNode_WhenNodeHaveTwoChildNodes()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		IGraph graph = Substitute.For<IGraph>();

		var context = new DataFlowExecutionContext
		{
			Graph = graph,
			DataFlowExecutionId = dataFlowExecutionId,
			DataFlowVersion = new DataFlowVersion(1),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
		_dataFlowExecutionContextProvider.GetRunningDataFlowExecutionContextAsync(dataFlowExecutionId, CancellationToken.None)
			.Returns(context);

		var readyNodeId = new NodeId(Guid.NewGuid());
		var notReadyNodeId = new NodeId(Guid.NewGuid());

		_childNodesFinder.GetChildNodeIds(nodeId, graph)
			.Returns(new HashSet<NodeId> { readyNodeId, notReadyNodeId });

		_nodeExecutionReadinessChecker.IsNodeReadyToExecute(context, readyNodeId).Returns(true);
		_nodeExecutionReadinessChecker.IsNodeReadyToExecute(context, notReadyNodeId).Returns(false);

		// Act
		await _sut.RunReadyChildNodes(dataFlowExecutionId, nodeId, CancellationToken.None);

		// Assert
		await _nodeExecutor.Received().ExecuteNodesAsync(dataFlowExecutionId, Arg.Is<IReadOnlySet<NodeId>>(nodes =>
			nodes.SequenceEqual(new[] { readyNodeId })), CancellationToken.None);
	}
}
