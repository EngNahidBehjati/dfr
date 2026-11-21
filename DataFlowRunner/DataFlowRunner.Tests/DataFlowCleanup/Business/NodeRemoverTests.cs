using DataFlowRunner.Common.Exceptions.NotFound;
using DataFlowRunner.DataFlowCleanup.Business;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.Tests.Common;

namespace DataFlowRunner.Tests.DataFlowCleanup.Business;

public sealed class NodeRemoverTests
{
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly INodeExecutionStateStore _nodeExecutionStateStore;
	private readonly NodeRemover _sut;

	public NodeRemoverTests()
	{
		_dataFlowExecutionContextProvider = Substitute.For<IDataFlowExecutionContextProvider>();
		_nodeExecutionStateStore = Substitute.For<INodeExecutionStateStore>();
		_sut = new NodeRemover(_dataFlowExecutionContextProvider, _nodeExecutionStateStore);
	}

	[Fact]
	public async Task RemoveNodesAsync_ShouldRemoveNodesFromGraphAndExecutionStates_WhenNodesAreProvided()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId1 = NodeId.New();
		var nodeId2 = NodeId.New();
		var nodeIds = new List<NodeId> { nodeId1, nodeId2 };

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

		// Act
		await _sut.RemoveNodesAsync(executionId, nodeIds, CancellationToken.None);

		// Assert
		graph.Received(1).RemoveNodes(Arg.Is<IReadOnlyCollection<NodeId>>(nodes =>
			nodes.SequenceEqual(nodeIds)));

		_nodeExecutionStateStore.Received(1).RemoveNodeExecutionState(executionId, nodeId1);
		_nodeExecutionStateStore.Received(1).RemoveNodeExecutionState(executionId, nodeId2);
	}

	[Fact]
	public async Task RemoveNodesAsync_ShouldNotRemoveStates_WhenExecutionContextCannotBeRetrieved()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = NodeId.New();
		var nodeIds = new List<NodeId> { nodeId };

		_dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(executionId, CancellationToken.None)
			.Returns<Task<DataFlowExecutionContext>>(
				_ => throw new DataFlowExecutionNotFoundException("Context not found"));

		// Act
		Func<Task> act = () => _sut.RemoveNodesAsync(executionId, nodeIds, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<DataFlowExecutionNotFoundException>().WithMessage("Context not found");

		_nodeExecutionStateStore.DidNotReceiveWithAnyArgs()
			.RemoveNodeExecutionState(default, default);
	}
}
