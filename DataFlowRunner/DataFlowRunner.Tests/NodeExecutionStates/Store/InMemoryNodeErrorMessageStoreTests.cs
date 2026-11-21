using DataFlowRunner.Common.Exceptions.NotFound;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Exceptions;
using DataFlowRunner.NodeExecutionStates.Models;
using DataFlowRunner.NodeExecutionStates.Store;
using FluentAssertions;

namespace DataFlowRunner.Tests.NodeExecutionStates.Store;

public class InMemoryNodeErrorMessageStoreTests
{
	private readonly InMemoryNodeErrorMessageStore _sut;

	public InMemoryNodeErrorMessageStoreTests()
	{
		_sut = new InMemoryNodeErrorMessageStore(LanguageDictionaryProvider.Instance);
	}

	[Fact]
	public void SetNodeErrorMessage_ShouldAddMessage_WhenNodeIsNew()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var message = new NodeErrorMessage { Message = "error occurred", StackTrace = "error stack trace" };

		// Act
		_sut.SetNodeErrorMessage(executionId, nodeId, message);

		// Assert
		_sut.GetNodeErrorMessage(executionId, nodeId).Should().Be(message);
	}

	[Fact]
	public void SetNodeErrorMessage_ShouldUpdateMessage_WhenNodeExists()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var oldMessage = new NodeErrorMessage { Message = "old", StackTrace = "old stack trace" };
		_sut.SetNodeErrorMessage(executionId, nodeId, oldMessage);
		var updatedMessage = new NodeErrorMessage { Message = "new", StackTrace = "new stack trace" };

		// Act
		_sut.SetNodeErrorMessage(executionId, nodeId, updatedMessage);

		// Assert
		_sut.GetNodeErrorMessage(executionId, nodeId).Should().Be(updatedMessage);
	}

	[Fact]
	public void GetNodeErrorMessage_ShouldThrowDataFlowExecutionNotFoundException_WhenDataFlowExecutionMissing()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();

		// Act
		Action act = () => _sut.GetNodeErrorMessage(executionId, nodeId);

		// Assert
		act.Should().Throw<DataFlowExecutionNotFoundException>();
	}

	[Fact]
	public void GetNodeErrorMessage_ShouldThrowNodeErrorMessageNotFoundException_WhenNodeMissing()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var existingNode = Guid.NewGuid();
		var missingNode = Guid.NewGuid();
		var nodeErrorMessage = new NodeErrorMessage { Message = "present", StackTrace = "missing stack trace" };
		_sut.SetNodeErrorMessage(executionId, existingNode, nodeErrorMessage);

		// Act
		Action act = () => _sut.GetNodeErrorMessage(executionId, missingNode);

		// Assert
		act.Should().Throw<NodeErrorMessageNotFoundException>();
	}
}
