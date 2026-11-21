using DataFlowRunner.DataFlowChangeEvents.Business.Handlers;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Tests.Common;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowChangeEvents.Business.Handlers;

public sealed class RemoveEdgeHandlerTests
{
	private readonly IDataFlowInvalidator _dataFlowInvalidator;
	private readonly RemoveEdgeHandler _sut;

	public RemoveEdgeHandlerTests()
	{
		_dataFlowInvalidator = Substitute.For<IDataFlowInvalidator>();
		_sut = new RemoveEdgeHandler(_dataFlowInvalidator);
	}

	[Fact]
	public async Task HandleAsync_ShouldInvalidateDownstreamAndRemoveEdge_WhenEdgeExists()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var runningVersion = new DataFlowVersion(5);
		var newVersion = new DataFlowVersion(30);

		var edgeId = EdgeId.New();
		var @event = new RemoveEdge(edgeId.Value);

		IGraph graph = Substitute.For<IGraph>();
		var downstreamNodeId = new NodeId(Guid.NewGuid());
		graph.GetEdges(Arg.Is<IReadOnlyCollection<EdgeId>>(edges => edges.SequenceEqual(new[] { edgeId })))
			.Returns([
				new EdgeByNode
				{
					Id = edgeId,
					FromNodeId = NodeId.New(),
					ToNodeId = downstreamNodeId
				}
			]);
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
			downstreamNodeId,
			CancellationToken.None);

		graph.Received(1).RemoveEdges(Arg.Is<IReadOnlyCollection<EdgeId>>(
			edges => edges.SequenceEqual(new[] { edgeId })));
	}
}
