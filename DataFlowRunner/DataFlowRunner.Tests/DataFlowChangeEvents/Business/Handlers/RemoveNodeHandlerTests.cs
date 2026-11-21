using DataFlowRunner.DataFlowChangeEvents.Business.Handlers;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;
using DataFlowRunner.DataFlowCleanup.Abstraction;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Tests.Common;

namespace DataFlowRunner.Tests.DataFlowChangeEvents.Business.Handlers;

public sealed class RemoveNodeHandlerTests
{
	private readonly IDataFlowInvalidator _dataFlowInvalidator;
	private readonly INodeRemover _nodeRemover;
	private readonly RemoveNodeHandler _sut;

	public RemoveNodeHandlerTests()
	{
		_dataFlowInvalidator = Substitute.For<IDataFlowInvalidator>();
		_nodeRemover = Substitute.For<INodeRemover>();
		_sut = new RemoveNodeHandler(_dataFlowInvalidator, _nodeRemover);
	}

	[Fact]
	public async Task HandleAsync_ShouldInvalidateDownstreamAndRemoveNode_WhenNodeExists()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var runningVersion = new DataFlowVersion(5);
		var newVersion = new DataFlowVersion(20);

		var nodeId = NodeId.New();
		var @event = new RemoveNode(nodeId.Value);

		IGraph graph = Substitute.For<IGraph>();
		graph.NodeExists(nodeId).Returns(true);
		ProcessorInfo processorInfo = DefaultProcessorInfoCreator.Create();
		var changeContext = new DataFlowChangeContext(
			executionId,
			runningVersion,
			newVersion,
			graph,
			processorInfo,
			DefaultExecutionSettingsCreator.Create());

		// Act
		await _sut.HandleAsync(changeContext, @event, CancellationToken.None);

		// Assert
		await _dataFlowInvalidator.Received(1).InvalidateNodeAndDownstreamAsync(
			Arg.Is<DataFlowExecutionContext>(context =>
				context.Graph == graph &&
				context.DataFlowExecutionId == executionId &&
				context.DataFlowVersion == runningVersion),
			nodeId,
			CancellationToken.None);

		await _nodeRemover.Received(1).RemoveNodesAsync(executionId, Arg.Is<List<NodeId>>(nodes =>
			nodes.SequenceEqual(new List<NodeId> { nodeId })), CancellationToken.None);
	}
}
