using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Business;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.NodeExecutionStates.Business;

public sealed class NodeExecutionErrorRecorderTests
{
	private readonly INodeExecutionStateUpdater _nodeExecutionStateUpdater;
	private readonly INodeErrorMessageStore _nodeErrorMessageStore;
	private readonly NodeExecutionErrorRecorder _sut;

	public NodeExecutionErrorRecorderTests()
	{
		_nodeExecutionStateUpdater = Substitute.For<INodeExecutionStateUpdater>();
		_nodeErrorMessageStore = Substitute.For<INodeErrorMessageStore>();
		_sut = new NodeExecutionErrorRecorder(_nodeExecutionStateUpdater, _nodeErrorMessageStore);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenNodeExecutionStateUpdaterIsNull()
	{
		// Arrange
		INodeExecutionStateUpdater nodeExecutionStateUpdater = null!;
		INodeErrorMessageStore nodeErrorMessageStore = Substitute.For<INodeErrorMessageStore>();

		// Act
		Action act = () => _ = new NodeExecutionErrorRecorder(nodeExecutionStateUpdater, nodeErrorMessageStore);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("nodeExecutionStateUpdater");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenNodeErrorMessageStoreIsNull()
	{
		// Arrange
		INodeExecutionStateUpdater nodeExecutionStateUpdater = Substitute.For<INodeExecutionStateUpdater>();
		INodeErrorMessageStore nodeErrorMessageStore = null!;

		// Act
		Action act = () => _ = new NodeExecutionErrorRecorder(nodeExecutionStateUpdater, nodeErrorMessageStore);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("nodeErrorMessageStore");
	}

	[Fact]
	public void RecordFailure_ShouldUpdateNodeStateToFailedAndStoreMessage_WhenCalled()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		var errorMessage = new NodeErrorMessage { Message = "Node execution failed", StackTrace = "stack trace" };

		// Act
		_sut.RecordFailure(executionId, nodeId, errorMessage);

		// Assert
		_nodeExecutionStateUpdater.Received(1)
			.ChangeNodeExecutionState(executionId, nodeId, NodeExecutionState.Failed);
		_nodeErrorMessageStore.Received(1)
			.SetNodeErrorMessage(executionId.Value, nodeId.Value, errorMessage);
	}

	[Fact]
	public void RecordCancellation_ShouldUpdateNodeStateToCanceledAndStoreMessage_WhenCalled()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		var errorMessage = new NodeErrorMessage { Message = "Node execution was canceled", StackTrace = null};

		// Act
		_sut.RecordCancellation(executionId, nodeId, errorMessage);

		// Assert
		_nodeExecutionStateUpdater.Received(1)
			.ChangeNodeExecutionState(executionId, nodeId, NodeExecutionState.Canceled);

		_nodeErrorMessageStore.Received(1)
			.SetNodeErrorMessage(executionId.Value, nodeId.Value, errorMessage);
	}
}
