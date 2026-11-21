using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Business;
using DataFlowRunner.NodeExecutionStates.Models;
using NSubstitute;

namespace DataFlowRunner.Tests.NodeExecutionStates.Business;

public class NodeExecutionTimeRecorderTests
{
	private readonly INodeExecutionTimingStore _nodeExecutionTimingStore;
	private readonly INodeExecutionStateClassifier _nodeExecutionStateClassifier;
	private readonly NodeExecutionTimeRecorder _sut;

	public NodeExecutionTimeRecorderTests()
	{
		_nodeExecutionTimingStore = Substitute.For<INodeExecutionTimingStore>();
		_nodeExecutionStateClassifier = Substitute.For<INodeExecutionStateClassifier>();
		_sut = new NodeExecutionTimeRecorder(_nodeExecutionTimingStore, _nodeExecutionStateClassifier);
	}

	[Fact]
	public void RecordNodeExecutionTime_ShouldSetNodeExecutionStartTime_WhenStateChangesFromNotExecutingToExecuting()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		const NodeExecutionState nextState = NodeExecutionState.Pending;
		const NodeExecutionState lastState = NodeExecutionState.Idle;

		_nodeExecutionStateClassifier.IsExecuting(NodeExecutionState.Pending).Returns(true);
		_nodeExecutionStateClassifier.IsExecuting(NodeExecutionState.Idle).Returns(false);

		// Act
		_sut.RecordNodeExecutionTime(dataFlowExecutionId, nodeId, lastState, nextState);

		// Assert
		_nodeExecutionTimingStore.Received(1).SetNodeExecutionStartTime(dataFlowExecutionId, nodeId);
		_nodeExecutionTimingStore.DidNotReceive().SetNodeExecutionEndTime(dataFlowExecutionId, nodeId);
	}

	[Fact]
	public void RecordNodeExecutionTime_ShouldSetNodeExecutionEndTime_WhenStateChangesFromExecutingToNotExecuting()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		const NodeExecutionState nextState = NodeExecutionState.Idle;
		const NodeExecutionState lastState = NodeExecutionState.Pending;

		_nodeExecutionStateClassifier.IsExecuting(NodeExecutionState.Pending).Returns(true);
		_nodeExecutionStateClassifier.IsExecuting(NodeExecutionState.Idle).Returns(false);

		// Act
		_sut.RecordNodeExecutionTime(dataFlowExecutionId, nodeId, lastState, nextState);

		// Assert
		_nodeExecutionTimingStore.Received(1).SetNodeExecutionEndTime(dataFlowExecutionId, nodeId);
		_nodeExecutionTimingStore.DidNotReceive().SetNodeExecutionStartTime(dataFlowExecutionId, nodeId);
	}

	[Fact]
	public void RecordNodeExecutionTime_ShouldNotSetNodeExecutionTime_WhenStateRemainsExecuting()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		const NodeExecutionState nextState = NodeExecutionState.Pending;
		const NodeExecutionState lastState = NodeExecutionState.Running;

		_nodeExecutionStateClassifier.IsExecuting(NodeExecutionState.Pending).Returns(true);
		_nodeExecutionStateClassifier.IsExecuting(NodeExecutionState.Running).Returns(true);

		// Act
		_sut.RecordNodeExecutionTime(dataFlowExecutionId, nodeId, lastState, nextState);

		// Assert
		_nodeExecutionTimingStore.DidNotReceive().SetNodeExecutionEndTime(dataFlowExecutionId, nodeId);
		_nodeExecutionTimingStore.DidNotReceive().SetNodeExecutionStartTime(dataFlowExecutionId, nodeId);
	}

	[Fact]
	public void RecordNodeExecutionTime_ShouldNotSetNodeExecutionTime_WhenStateRemainsNotExecuting()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		const NodeExecutionState nextState = NodeExecutionState.Succeeded;
		const NodeExecutionState lastState = NodeExecutionState.Idle;

		_nodeExecutionStateClassifier.IsExecuting(NodeExecutionState.Succeeded).Returns(false);
		_nodeExecutionStateClassifier.IsExecuting(NodeExecutionState.Idle).Returns(false);

		// Act
		_sut.RecordNodeExecutionTime(dataFlowExecutionId, nodeId, lastState, nextState);

		// Assert
		_nodeExecutionTimingStore.DidNotReceive().SetNodeExecutionEndTime(dataFlowExecutionId, nodeId);
		_nodeExecutionTimingStore.DidNotReceive().SetNodeExecutionStartTime(dataFlowExecutionId, nodeId);
	}
}
