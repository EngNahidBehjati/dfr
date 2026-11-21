using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;
using DataFlowRunner.TableQuery.Business;
using DataFlowRunner.TableQuery.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.TableQuery.Business;

public sealed class NodeExecutionStateValidatorTests
{
	private readonly INodeExecutionStateProvider _nodeExecutionStateProvider;
	private readonly NodeExecutionStateValidator _sut;

	public NodeExecutionStateValidatorTests()
	{
		_nodeExecutionStateProvider = Substitute.For<INodeExecutionStateProvider>();

		_sut = new NodeExecutionStateValidator(_nodeExecutionStateProvider, LanguageDictionaryProvider.Instance);
	}

	[Fact]
	public void ValidateNodeIsSucceeded_ShouldNotThrow_WhenNodeIsSucceeded()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		_nodeExecutionStateProvider.GetNodeExecutionState(dataFlowExecutionId, nodeId)
			.Returns(NodeExecutionState.Succeeded);

		// Act
		Action act = () => _sut.ValidateNodeIsSucceeded(dataFlowExecutionId, nodeId);

		// Assert
		act.Should().NotThrow();
		_nodeExecutionStateProvider.Received(1).GetNodeExecutionState(dataFlowExecutionId, nodeId);
	}

	[Fact]
	public void ValidateNodeIsSucceeded_ShouldThrowNodeExecutionStateNotSucceededException_WhenNodeIsNotSucceeded()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		_nodeExecutionStateProvider.GetNodeExecutionState(dataFlowExecutionId, nodeId)
			.Returns(NodeExecutionState.Failed);

		// Act
		Action act = () => _sut.ValidateNodeIsSucceeded(dataFlowExecutionId, nodeId);

		// Assert
		act.Should().Throw<NodeExecutionStateNotSucceededException>();
	}
}
