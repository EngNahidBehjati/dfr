using DataFlowRunner.Common.Exceptions.NotFound;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Models;
using DataFlowRunner.NodeExecutionStates.Store;
using FluentAssertions;

namespace DataFlowRunner.Tests.NodeExecutionStates.Store;

public sealed class InMemoryNodeExecutionStateStoreTests
{
	private readonly InMemoryNodeExecutionStateStore _sut;

	public InMemoryNodeExecutionStateStoreTests()
	{
		_sut = new InMemoryNodeExecutionStateStore(LanguageDictionaryProvider.Instance);
	}

	[Fact]
	public void SetNodeExecutionState_ShouldAddState_WhenNodeIsNew()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var node = new NodeId(Guid.NewGuid());

		// Act
		_sut.SetNodeExecutionState(executionId, node, NodeExecutionState.Succeeded);

		// Assert
		NodeExecutionState state = _sut.GetNodeExecutionState(executionId, node);
		state.Should().Be(NodeExecutionState.Succeeded);
	}

	[Fact]
	public void SetNodeExecutionState_ShouldUpdateState_WhenNodeExists()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var node = new NodeId(Guid.NewGuid());
		_sut.SetNodeExecutionState(executionId, node, NodeExecutionState.Idle);

		// Act
		_sut.SetNodeExecutionState(executionId, node, NodeExecutionState.Running);

		// Assert
		NodeExecutionState state = _sut.GetNodeExecutionState(executionId, node);
		state.Should().Be(NodeExecutionState.Running);
	}

	[Fact]
	public void GetNodeExecutionState_ShouldThrowDataFlowExecutionNotFoundException_WhenDataFlowExecutionMissing()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var node = new NodeId(Guid.NewGuid());

		// Act
		Action act = () => _sut.GetNodeExecutionState(executionId, node);

		// Assert
		act.Should().Throw<DataFlowExecutionNotFoundException>();
	}

	[Fact]
	public void GetNodeExecutionState_ShouldThrowNodeNotFoundException_WhenNodeMissing()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var existingNode = new NodeId(Guid.NewGuid());
		var missingNode = new NodeId(Guid.NewGuid());
		_sut.SetNodeExecutionState(executionId, existingNode, NodeExecutionState.Idle);

		// Act
		Action act = () => _sut.GetNodeExecutionState(executionId, missingNode);

		// Assert
		act.Should().Throw<NodeNotFoundException>();
	}

	[Fact]
	public void TryGetNodeExecutionState_ShouldReturnFalse_WhenNodeMissing()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var existingNode = new NodeId(Guid.NewGuid());
		var missingNode = new NodeId(Guid.NewGuid());
		_sut.SetNodeExecutionState(executionId, existingNode, NodeExecutionState.Idle);

		// Act
		var result = _sut.TryGetNodeExecutionState(executionId, missingNode, out NodeExecutionState? state);

		// Assert
		result.Should().BeFalse();
		state.Should().BeNull();
	}

	[Fact]
	public void TryGetNodeExecutionState_ShouldReturnFalse_WhenDataFlowExecutionMissing()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var node = new NodeId(Guid.NewGuid());

		// Act
		var result = _sut.TryGetNodeExecutionState(executionId, node, out NodeExecutionState? state);

		// Assert
		result.Should().BeFalse();
		state.Should().BeNull();
	}

	[Fact]
	public void GetNodeExecutionState_ShouldReturnTrue_WhenNodeStateExists()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var node = new NodeId(Guid.NewGuid());
		_sut.SetNodeExecutionState(executionId, node, NodeExecutionState.Succeeded);

		// Act
		var result = _sut.TryGetNodeExecutionState(executionId, node, out NodeExecutionState? state);

		// Assert
		result.Should().BeTrue();
		state.Should().Be(NodeExecutionState.Succeeded);
	}

	[Fact]
	public void GetAllNodeExecutionStates_ShouldReturnEmptyDictionary_WhenDataFlowExecutionMissing()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());

		// Act
		IReadOnlyDictionary<NodeId, NodeExecutionState> result = _sut.GetAllNodeExecutionStates(executionId);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void GetAllNodeExecutionStates_ShouldReturnAllStates_WhenDataFlowExecutionExists()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId1 = new NodeId(Guid.NewGuid());
		var nodeId2 = new NodeId(Guid.NewGuid());
		var nodeId3 = new NodeId(Guid.NewGuid());

		_sut.SetNodeExecutionState(executionId, nodeId1, NodeExecutionState.Idle);
		_sut.SetNodeExecutionState(executionId, nodeId2, NodeExecutionState.Running);
		_sut.SetNodeExecutionState(executionId, nodeId3, NodeExecutionState.Succeeded);

		var expectedStates = new Dictionary<NodeId, NodeExecutionState>
		{
			[nodeId1] = NodeExecutionState.Idle,
			[nodeId2] = NodeExecutionState.Running,
			[nodeId3] = NodeExecutionState.Succeeded
		};

		// Act
		IReadOnlyDictionary<NodeId, NodeExecutionState> result = _sut.GetAllNodeExecutionStates(executionId);

		// Assert
		result.Should().BeEquivalentTo(expectedStates);
	}
}
