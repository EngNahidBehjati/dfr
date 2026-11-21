using DataFlowRunner.NodeExecutionStates.DataTransferObjects;
using DataFlowRunner.NodeExecutionStates.Mappers;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;

namespace DataFlowRunner.Tests.NodeExecutionStates.Mappers;

public sealed class NodeErrorMessageMapperTests
{
	private readonly NodeErrorMessageMapper _sut = new();

	[Theory]
	[InlineData("", "")]
	[InlineData("Simple error message", "simple error stack trace")]
	[InlineData(
		"Very long error message with multiple words and detailed information about what went wrong during the node execution process"
		, "long error message stack trace")]
	public void Map_ShouldMapMessageToErrorMessageAndStackTraceToErrorStackTrace_WhenGivenDifferentMessages(
		string message, string stackTrace)
	{
		// Arrange
		var nodeErrorMessage = new NodeErrorMessage { Message = message, StackTrace = stackTrace };

		// Act
		NodeErrorMessageDto result = _sut.Map(nodeErrorMessage);

		// Assert
		result.ErrorMessage.Should().Be(message);
	}
}
