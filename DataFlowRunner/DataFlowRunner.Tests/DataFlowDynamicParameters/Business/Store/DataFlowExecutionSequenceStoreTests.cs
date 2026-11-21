using DataFlowRunner.DataFlowDynamicParameters.Business.Store;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using FluentAssertions;

namespace DataFlowRunner.Tests.DataFlowDynamicParameters.Business.Store;

public sealed class DataFlowExecutionSequenceStoreTests
{
	private readonly DataFlowExecutionSequenceStore _sut = new();

	[Fact]
	public void GetNodeRunSequence_ShouldReturn0_WhenExecutionDoesNotExist()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());

		// Act
		var actual = _sut.GetNodeRunSequence(executionId, nodeId);

		// Assert
		actual.Should().Be(0);
	}

	[Fact]
	public void GetNodeRunSequence_ShouldReturn0_WhenNodeWasNeverRecorded()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var submittedNode = new NodeId(Guid.NewGuid());
		var unknownNode = new NodeId(Guid.NewGuid());

		_sut.SetNextRunSequence(executionId, [submittedNode]);

		// Act
		var actual = _sut.GetNodeRunSequence(executionId, unknownNode);

		// Assert
		actual.Should().Be(0);
	}

	[Fact]
	public void SetNextRunSequence_ShouldAssignSameIncrementalSequenceToAllNodes_WhenCalled()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());

		// Act
		_sut.SetNextRunSequence(executionId, [nodeA, nodeB]);

		// Assert
		var sequenceA = _sut.GetNodeRunSequence(executionId, nodeA);
		var sequenceB = _sut.GetNodeRunSequence(executionId, nodeB);
		sequenceA.Should().Be(1);
		sequenceB.Should().Be(1);
	}
}
