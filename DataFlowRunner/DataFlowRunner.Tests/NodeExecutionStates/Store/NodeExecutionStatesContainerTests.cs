using DataFlowRunner.Common.Exceptions.NotFound;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Models;
using DataFlowRunner.NodeExecutionStates.Store;
using FluentAssertions;

namespace DataFlowRunner.Tests.NodeExecutionStates.Store;

public sealed class NodeExecutionStatesContainerTests
{
	private readonly NodeExecutionStatesContainer _sut;

	public NodeExecutionStatesContainerTests()
	{
		_sut = new NodeExecutionStatesContainer(LanguageDictionaryProvider.Instance);
	}

	[Fact]
	public void GetAllStates_ShouldReturnEmptyDictionary_WhenNoStatesAdded()
	{
		// Act
		IReadOnlyDictionary<NodeId, NodeExecutionState> result = _sut.GetAllStates();

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void GetAllStates_ShouldReturnAllStates_WhenStatesAreAdded()
	{
		// Arrange
		var nodeId1 = new NodeId(Guid.NewGuid());
		var nodeId2 = new NodeId(Guid.NewGuid());
		var nodeId3 = new NodeId(Guid.NewGuid());

		_sut.Set(nodeId1, NodeExecutionState.Idle);
		_sut.Set(nodeId2, NodeExecutionState.Running);
		_sut.Set(nodeId3, NodeExecutionState.Succeeded);

		var expectedStates = new Dictionary<NodeId, NodeExecutionState>
		{
			{ nodeId1, NodeExecutionState.Idle },
			{ nodeId2, NodeExecutionState.Running },
			{ nodeId3, NodeExecutionState.Succeeded }
		};

		// Act
		IReadOnlyDictionary<NodeId, NodeExecutionState> result = _sut.GetAllStates();

		// Assert
		result.Should().BeEquivalentTo(expectedStates);
	}

	[Fact]
	public void Get_ShouldThrowNodeNotFoundException_WhenNodeDoesNotExist()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());

		// Act
		Action act = () => _sut.Get(nodeId);

		// Assert
		act.Should().Throw<NodeNotFoundException>();
	}

	[Fact]
	public void Get_ShouldReturnState_WhenNodeExists()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());
		_sut.Set(nodeId, NodeExecutionState.Pending);

		// Act
		NodeExecutionState result = _sut.Get(nodeId);

		// Assert
		result.Should().Be(NodeExecutionState.Pending);
	}

	[Fact]
	public void TryGet_ShouldReturnFalse_WhenNodeDoesNotExist()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());

		// Act
		var result = _sut.TryGet(nodeId, out NodeExecutionState? state);

		// Assert
		result.Should().BeFalse();
		state.Should().BeNull();
	}

	[Fact]
	public void TryGet_ShouldSetStateAndReturnTrue_WhenNodeExists()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());
		_sut.Set(nodeId, NodeExecutionState.Pending);

		// Act
		var result = _sut.TryGet(nodeId, out NodeExecutionState? state);

		// Assert
		result.Should().BeTrue();
		state.Should().Be(NodeExecutionState.Pending);
	}

	[Fact]
	public void Set_ShouldUpdateState_WhenStateWasSet()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());

		// Act
		_sut.Set(nodeId, NodeExecutionState.Idle);
		_sut.Set(nodeId, NodeExecutionState.Running);

		// Assert
		NodeExecutionState result = _sut.Get(nodeId);
		result.Should().Be(NodeExecutionState.Running);
	}
}

