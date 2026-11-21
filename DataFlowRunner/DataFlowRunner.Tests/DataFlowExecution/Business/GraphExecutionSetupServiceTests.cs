using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecution.Business;

public class GraphExecutionSetupServiceTests
{
	private readonly INodeExecutorEngineGraphInitializer _nodeExecutorEngineGraphInitializer;
	private readonly INodeConfigInitializer _nodeConfigInitializer;
	private readonly INodeExecutionStateUpdater _nodeExecutionStateUpdater;
	private readonly IDataFlowExecutionStatusStore _dataFlowExecutionStatusStore;
	private readonly GraphExecutionSetupService _sut;

	public GraphExecutionSetupServiceTests()
	{
		_nodeExecutorEngineGraphInitializer = Substitute.For<INodeExecutorEngineGraphInitializer>();
		_nodeConfigInitializer = Substitute.For<INodeConfigInitializer>();
		_nodeExecutionStateUpdater = Substitute.For<INodeExecutionStateUpdater>();
		_dataFlowExecutionStatusStore = Substitute.For<IDataFlowExecutionStatusStore>();

		_sut = new GraphExecutionSetupService(
			_nodeConfigInitializer,
			_nodeExecutionStateUpdater,
			_nodeExecutorEngineGraphInitializer,
			_dataFlowExecutionStatusStore);
	}

	[Fact]
	public async Task SetupExecution_ShouldLoadConfigurationsAndSetNodeStatesToPending_WhenCalled()
	{
		// Arrange
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();

		var nodeA = new Node { Id = new NodeId(Guid.NewGuid()) };
		var nodeB = new Node { Id = new NodeId(Guid.NewGuid()) };
		var nodes = new HashSet<Node> { nodeA, nodeB };
		graph.GetAllNodes().Returns(nodes);
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());

		// Act
		await _sut.SetupExecution(graph, dataFlowExecutionId, CancellationToken.None);

		// Assert
		await _nodeExecutorEngineGraphInitializer.Received().TryInitialize(dataFlowExecutionId, Arg.Any<CancellationToken>());
		_dataFlowExecutionStatusStore.Received(1).Set(dataFlowExecutionId,
			Arg.Is<DataFlowExecutionStatus>(status => status.Failed == false && status.FailureMessage == null));
		await _nodeConfigInitializer.Received().EnsureNodeConfigsLoadedAsync(dataFlowExecutionId, nodes, CancellationToken.None);
		_nodeExecutionStateUpdater.Received().ChangeNodeExecutionState(dataFlowExecutionId, nodeA.Id, NodeExecutionState.Pending);
		_nodeExecutionStateUpdater.Received().ChangeNodeExecutionState(dataFlowExecutionId, nodeB.Id, NodeExecutionState.Pending);
	}
}
