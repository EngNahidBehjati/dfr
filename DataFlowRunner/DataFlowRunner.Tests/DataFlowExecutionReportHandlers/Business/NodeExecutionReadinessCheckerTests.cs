using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionReportHandlers.Business;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;
using DataFlowRunner.Tests.Common;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecutionReportHandlers.Business;

public sealed class NodeExecutionReadinessCheckerTests
{
	private readonly INodeExecutionStateProvider _nodeExecutionStateProvider;
	private readonly NodeExecutionReadinessChecker _sut;

	public NodeExecutionReadinessCheckerTests()
	{
		_nodeExecutionStateProvider = Substitute.For<INodeExecutionStateProvider>();
		_sut = new NodeExecutionReadinessChecker(_nodeExecutionStateProvider);
	}

	[Fact]
	public void IsNodeReadyToExecute_ShouldThrowArgumentNullException_WhenContextIsNull()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());

		// Act
		Action act = () => _sut.IsNodeReadyToExecute(null!, nodeId);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void IsNodeReadyToExecute_ShouldReturnTrue_WhenNodeStateIsPendingAndAllParentNodeStatesAreSucceeded()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());
		var parentNodeId1 = new NodeId(Guid.NewGuid());
		var parentNodeId2 = new NodeId(Guid.NewGuid());

		IGraph graph = Substitute.For<IGraph>();
		graph.GetAllEdges().Returns(new HashSet<EdgeByNode>
		{
			new()
			{
				Id = new EdgeId(Guid.NewGuid()),
				FromNodeId = parentNodeId1,
				ToNodeId = nodeId
			},
			new()
			{
				Id = new EdgeId(Guid.NewGuid()),
				FromNodeId = parentNodeId2,
				ToNodeId = nodeId
			}
		});

		var executionId = new DataFlowExecutionId(Guid.NewGuid());

		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, parentNodeId1)
			.Returns(NodeExecutionState.Succeeded);

		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, parentNodeId2)
			.Returns(NodeExecutionState.Succeeded);

		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, nodeId)
			.Returns(NodeExecutionState.Pending);

		var context = new DataFlowExecutionContext
		{
			Graph = graph,
			DataFlowExecutionId = executionId,
			DataFlowVersion = new DataFlowVersion(1),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		// Act
		var result = _sut.IsNodeReadyToExecute(context, nodeId);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void IsNodeReadyToExecute_ShouldReturnFalse_WhenNodeExecutionStateIsPendingAndSomeParentNodeStatesAreNotSucceeded()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());
		var parentNodeId1 = new NodeId(Guid.NewGuid());
		var parentNodeId2 = new NodeId(Guid.NewGuid());

		IGraph graph = Substitute.For<IGraph>();
		graph.GetAllEdges().Returns(new HashSet<EdgeByNode>
		{
			new()
			{
				Id = new EdgeId(Guid.NewGuid()),
				FromNodeId = parentNodeId1,
				ToNodeId = nodeId
			},
			new()
			{
				Id = new EdgeId(Guid.NewGuid()),
				FromNodeId = parentNodeId2,
				ToNodeId = nodeId
			}
		});

		var executionId = new DataFlowExecutionId(Guid.NewGuid());

		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, parentNodeId1)
			.Returns(NodeExecutionState.Succeeded);

		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, parentNodeId2)
			.Returns(NodeExecutionState.Failed);

		_nodeExecutionStateProvider.GetNodeExecutionState(executionId, nodeId)
			.Returns(NodeExecutionState.Pending);

		var context = new DataFlowExecutionContext
		{
			Graph = graph,
			DataFlowExecutionId = executionId,
			DataFlowVersion = new DataFlowVersion(1),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		// Act
		var result = _sut.IsNodeReadyToExecute(context, nodeId);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void IsNodeReadyToExecute_ShouldReturnFalse_WhenNodeExecutionStateIsNotPending()
	{
		// Arrange
		var dataFlowExecutionId = DataFlowExecutionId.New();
		var nodeId = NodeId.New();
		IGraph graph = Substitute.For<IGraph>();
		var dataFlowExecutionContext = new DataFlowExecutionContext()
		{
			Graph = graph,
			DataFlowExecutionId = dataFlowExecutionId,
			DataFlowVersion = new DataFlowVersion(1),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
		graph.GetAllEdges().Returns(new HashSet<EdgeByNode>());
		_nodeExecutionStateProvider.GetNodeExecutionState(dataFlowExecutionId, nodeId)
			.Returns(NodeExecutionState.Idle);

		// Act
		var result = _sut.IsNodeReadyToExecute(dataFlowExecutionContext, nodeId);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void IsNodeReadyToExecute_ShouldReturnTrue_WhenNodeExecutionStateIsPendingAndNodeHasNoParents()
	{
		// Arrange
		var dataFlowExecutionId = DataFlowExecutionId.New();
		var nodeId = new NodeId(Guid.NewGuid());

		IGraph graph = Substitute.For<IGraph>();
		graph.GetAllEdges().Returns(new HashSet<EdgeByNode>());
		_nodeExecutionStateProvider.GetNodeExecutionState(dataFlowExecutionId, nodeId)
			.Returns(NodeExecutionState.Pending);

		var context = new DataFlowExecutionContext
		{
			Graph = graph,
			DataFlowExecutionId = dataFlowExecutionId,
			DataFlowVersion = new DataFlowVersion(1),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		// Act
		var result = _sut.IsNodeReadyToExecute(context, nodeId);

		// Assert
		result.Should().BeTrue();
	}
}
