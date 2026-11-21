using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Exceptions;
using DataFlowRunner.NodeExecutionStates.Models;
using DataFlowRunner.NodeExecutionStates.Store;
using FluentAssertions;

namespace DataFlowRunner.Tests.NodeExecutionStates.Store;

public class NodeErrorMessagesContainerTests
{
	private readonly NodeErrorMessagesContainer _sut;

	public NodeErrorMessagesContainerTests()
	{
		_sut = new NodeErrorMessagesContainer(LanguageDictionaryProvider.Instance);
	}

	[Fact]
	public void Set_ShouldStoreMessage_WhenNodeIsNew()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());
		var message = new NodeErrorMessage { Message = "error", StackTrace = "stacktrace" };

		// Act
		_sut.SetMessage(nodeId, message);

		// Assert
		_sut.GetMessage(nodeId).Should().Be(message);
	}

	[Fact]
	public void Set_ShouldOverwriteExistingMessage_WhenNodeExists()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());
		_sut.SetMessage(nodeId, new NodeErrorMessage { Message = "old", StackTrace = "old stacktrace" });
		var newMessage = new NodeErrorMessage { Message = "new", StackTrace = "new stacktrace" };

		// Act
		_sut.SetMessage(nodeId, newMessage);

		// Assert
		_sut.GetMessage(nodeId).Should().Be(newMessage);
	}

	[Fact]
	public void Get_ShouldThrowNodeErrorMessageNotFoundException_WhenNodeMissing()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());

		// Act
		Action act = () => _sut.GetMessage(nodeId);

		// Assert
		act.Should().Throw<NodeErrorMessageNotFoundException>();
	}
}
