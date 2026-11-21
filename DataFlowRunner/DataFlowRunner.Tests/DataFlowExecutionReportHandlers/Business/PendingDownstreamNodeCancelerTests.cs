using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionReportHandlers.Business;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.GraphTraversal.Abstractions;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;
using DataFlowRunner.Tests.Common;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecutionReportHandlers.Business;

public sealed class PendingDownstreamNodeCancelerTests
{
	private readonly INodeExecutionErrorRecorder _nodeExecutionErrorRecorder;
	private readonly IDownstreamSubgraphDetector _downstreamSubgraphDetector;
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly PendingDownstreamNodeCanceler _sut;

	public PendingDownstreamNodeCancelerTests()
	{
		_nodeExecutionErrorRecorder = Substitute.For<INodeExecutionErrorRecorder>();
		_downstreamSubgraphDetector = Substitute.For<IDownstreamSubgraphDetector>();
		_dataFlowExecutionContextProvider = Substitute.For<IDataFlowExecutionContextProvider>();
		_sut = new PendingDownstreamNodeCanceler(
			_nodeExecutionErrorRecorder, _downstreamSubgraphDetector, _dataFlowExecutionContextProvider);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenNodeExecutionErrorRecorderIsNull()
	{
		// Act
		Action act = () => _ = new PendingDownstreamNodeCanceler(
			null!, _downstreamSubgraphDetector, _dataFlowExecutionContextProvider);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("nodeExecutionErrorRecorder");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDownstreamSubgraphDetectorIsNull()
	{
		// Act
		Action act = () => _ = new PendingDownstreamNodeCanceler(
			_nodeExecutionErrorRecorder, null!, _dataFlowExecutionContextProvider);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("downstreamSubgraphDetector");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowExecutionContextProviderIsNull()
	{
		// Act
		Action act = () => _ = new PendingDownstreamNodeCanceler(
			_nodeExecutionErrorRecorder, _downstreamSubgraphDetector, null!);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("dataFlowExecutionContextProvider");
	}

	[Fact]
	public async Task CancelAsync_ShouldRecordCancellationForEachPendingDownstreamNode_WhenPendingDownstreamNodesExist()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var sourceNodeId = new NodeId(Guid.NewGuid());
		var pendingNodeId1 = new NodeId(Guid.NewGuid());
		var pendingNodeId2 = new NodeId(Guid.NewGuid());

		IGraph graph = Substitute.For<IGraph>();
		var context = new DataFlowExecutionContext
		{
			DataFlowExecutionId = executionId,
			DataFlowVersion = new DataFlowVersion(1),
			Graph = graph,
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		_dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(executionId, CancellationToken.None)
			.Returns(context);

		_downstreamSubgraphDetector
			.DetectDownstreamNodes(graph, executionId, sourceNodeId, Arg.Any<Func<NodeExecutionState, bool>>())
			.Returns(new HashSet<NodeId> { pendingNodeId1, pendingNodeId2 });

		// Act
		await _sut.CancelAsync(executionId, sourceNodeId, CancellationToken.None);

		// Assert
		await _dataFlowExecutionContextProvider.Received(1)
			.GetRunningDataFlowExecutionContextAsync(executionId, CancellationToken.None);
		_downstreamSubgraphDetector.Received(1)
			.DetectDownstreamNodes(graph, executionId, sourceNodeId, Arg.Any<Func<NodeExecutionState, bool>>());

		_nodeExecutionErrorRecorder.Received(1)
			.RecordCancellation(executionId, pendingNodeId1,
				Arg.Is<NodeErrorMessage>(e =>
					e.Message == "The plugin execution was canceled by the previous plugin." &&
					e.StackTrace == null));

		_nodeExecutionErrorRecorder.Received(1)
			.RecordCancellation(executionId, pendingNodeId2,
				Arg.Is<NodeErrorMessage>(e =>
					e.Message == "The plugin execution was canceled by the previous plugin." &&
					e.StackTrace == null));
	}

	[Fact]
	public async Task CancelAsync_ShouldNotRecordCancellation_WhenNoPendingDownstreamNodesExist()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var sourceNodeId = new NodeId(Guid.NewGuid());

		IGraph graph = Substitute.For<IGraph>();
		var context = new DataFlowExecutionContext
		{
			DataFlowExecutionId = executionId,
			DataFlowVersion = new DataFlowVersion(1),
			Graph = graph,
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		_dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(executionId, CancellationToken.None)
			.Returns(context);

		_downstreamSubgraphDetector
			.DetectDownstreamNodes(graph, executionId, sourceNodeId, Arg.Any<Func<NodeExecutionState, bool>>())
			.Returns(new HashSet<NodeId>());

		// Act
		await _sut.CancelAsync(executionId, sourceNodeId, CancellationToken.None);

		// Assert
		await _dataFlowExecutionContextProvider.Received(1)
			.GetRunningDataFlowExecutionContextAsync(executionId, CancellationToken.None);

		_nodeExecutionErrorRecorder.DidNotReceive()
			.RecordCancellation(Arg.Any<DataFlowExecutionId>(), Arg.Any<NodeId>(), Arg.Any<NodeErrorMessage>());
	}
}
