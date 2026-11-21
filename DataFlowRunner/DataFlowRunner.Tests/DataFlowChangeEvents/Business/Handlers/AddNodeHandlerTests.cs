using DataFlowRunner.DataFlowChangeEvents.Business.Handlers;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.Tests.Common;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowChangeEvents.Business.Handlers;

public sealed class AddNodeHandlerTests
{
	private readonly AddNodeHandler _sut;
	private readonly INodeExecutionStateUpdater _nodeExecutionStateUpdater;

	public AddNodeHandlerTests()
	{
		_nodeExecutionStateUpdater = Substitute.For<INodeExecutionStateUpdater>();
		_sut = new AddNodeHandler(_nodeExecutionStateUpdater);
	}

	[Fact]
	public async Task HandleAsync_ShouldAddNodeToGraphAndAddNodeState_WhenCalled()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var runningVersion = new DataFlowVersion(5);
		var newVersion = new DataFlowVersion(10);

		var nodeId = NodeId.New();
		var @event = new AddNode(NodeInfo: new NodeInfo(
			nodeId.Value,
			"test",
			"PACKAGE",
			1,
			new Dictionary<string, IEnumerable<PortInfo>>()));

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
		graph.Received(1).AddNodes(Arg.Is<IReadOnlyCollection<NodeId>>(nodes =>
				nodes.Count == 1 && nodes.Single().Equals(nodeId)));

		_nodeExecutionStateUpdater.Received().TryAddNodeExecutionState(executionId, nodeId);
	}
}
