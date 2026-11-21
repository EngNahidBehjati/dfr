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

public sealed class AddEdgeHandlerTests
{
	private readonly IDataFlowInvalidator _dataFlowInvalidator;
	private readonly AddEdgeHandler _sut;

	public AddEdgeHandlerTests()
	{
		_dataFlowInvalidator = Substitute.For<IDataFlowInvalidator>();
		_sut = new AddEdgeHandler(_dataFlowInvalidator);
	}

	[Fact]
	public async Task HandleAsync_ShouldAddEdgeAndInvalidateDownstream_WhenCalled()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var runningVersion = new DataFlowVersion(7);
		var newVersion = new DataFlowVersion(12);

		var edgeId = EdgeId.New();
		var fromNodeId = NodeId.New();
		var toNodeId = NodeId.New();
		var @event = new AddEdge(new EdgeInfo(
			edgeId.Value,
			Guid.NewGuid(),
			Guid.NewGuid(),
			fromNodeId.Value,
			toNodeId.Value));

		IGraph graph = Substitute.For<IGraph>();
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
			toNodeId,
			CancellationToken.None);

		graph.Received(1)
			.AddEdges(Arg.Is<IReadOnlyCollection<EdgeByNode>>(edges =>
				edges.Count == 1 &&
				edges.Single().Id.Equals(edgeId) &&
				edges.Single().FromNodeId.Equals(fromNodeId) &&
				edges.Single().ToNodeId.Equals(toNodeId)));
	}
}
