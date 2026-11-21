using DataFlowRunner.DataFlowChangeEvents.Business.Handlers;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Tests.Common;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowChangeEvents.Business.Handlers;

public sealed class SetNodeConfigHandlerTests
{
	private readonly IDataFlowInvalidator _invalidator;
	private readonly SetNodeConfigHandler _sut;

	public SetNodeConfigHandlerTests()
	{
		_invalidator = Substitute.For<IDataFlowInvalidator>();
		_sut = new SetNodeConfigHandler(_invalidator);
	}

	[Fact]
	public async Task HandleAsync_ShouldInvalidateNodeAndDownstream_WhenNodeExists()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var runningVersion = new DataFlowVersion(1);
		var newVersion = new DataFlowVersion(50);

		var nodeId = NodeId.New();
		var @event = new SetNodeConfig(
			nodeId.Value,
			[
				new DynamicOutputPortInfo(Guid.NewGuid(), "out1")
			]);

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
		await _invalidator.Received(1).InvalidateNodeAndDownstreamAsync(
			Arg.Is<DataFlowExecutionContext>(context =>
				context.Graph == graph &&
				context.DataFlowExecutionId == executionId &&
				context.DataFlowVersion == runningVersion),
			nodeId,
			CancellationToken.None);
	}

	[Fact]
	public async Task HandleAsync_ShouldNotInvalidate_WhenNodeDoesNotExist()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var runningVersion = new DataFlowVersion(1);
		var newVersion = new DataFlowVersion(50);

		var nodeId = NodeId.New();
		var @event = new SetNodeConfig(nodeId.Value, []);

		IGraph graph = Substitute.For<IGraph>();
		graph.NodeExists(nodeId).Returns(false);
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
		await _invalidator.DidNotReceiveWithAnyArgs()
			.InvalidateNodeAndDownstreamAsync(null!, default, CancellationToken.None);
	}
}
