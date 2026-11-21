using DataFlowRunner.Common.Exceptions.NotFound;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Controllers;
using DataFlowRunner.NodeExecutionStates.DataTransferObjects;
using DataFlowRunner.NodeExecutionStates.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace DataFlowRunner.Tests.NodeExecutionStates.Controllers;

public class NodeStateErrorMessageControllerTests
{
	private readonly INodeErrorMessageDtoProvider _nodeErrorMessageDtoProvider;
	private readonly NodeStateErrorMessageController _sut;

	public NodeStateErrorMessageControllerTests()
	{
		_nodeErrorMessageDtoProvider = Substitute.For<INodeErrorMessageDtoProvider>();
		_sut = new NodeStateErrorMessageController(_nodeErrorMessageDtoProvider);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenFacadeIsNull()
	{
		// Arrange
		INodeErrorMessageDtoProvider facade = null!;

		// Act
		Action act = () => _ = new NodeStateErrorMessageController(facade);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("nodeErrorMessageDtoProvider");
	}

	[Fact]
	public async Task GetNodeErrorMessage_ShouldReturnOkWithPayload_WhenNodeErrorMessageFound()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var expectedDto = new NodeErrorMessageDto { ErrorMessage = "failure", StackTrace = "stack trace" };
		_nodeErrorMessageDtoProvider.GetNodeErrorMessageAsync(executionId, nodeId, CancellationToken.None)
			.Returns(Task.FromResult(expectedDto));

		// Act
		IActionResult result = await _sut.GetNodeErrorMessage(executionId, nodeId, CancellationToken.None);

		// Assert
		result.Should().BeOfType<OkObjectResult>();
		var okResult = (OkObjectResult)result;
		okResult.Value.Should().BeEquivalentTo(expectedDto);
	}

	[Fact]
	public void GetNodeErrorMessage_ShouldThrowDataFlowExecutionNotFound_WhenDataFlowExecutionMissing()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		_nodeErrorMessageDtoProvider.GetNodeErrorMessageAsync(executionId, nodeId, CancellationToken.None)
			.Returns<Task<NodeErrorMessageDto>>(_ => throw new DataFlowExecutionNotFoundException());

		// Act
		Func<Task> act = async () => await _sut.GetNodeErrorMessage(executionId, nodeId, CancellationToken.None);

		// Assert
		act.Should().ThrowAsync<DataFlowExecutionNotFoundException>();
	}

	[Fact]
	public void GetNodeErrorMessage_ShouldThrowNodeErrorMessageNotFound_WhenNodeErrorMessageMissing()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		_nodeErrorMessageDtoProvider.GetNodeErrorMessageAsync(executionId, nodeId, CancellationToken.None)
			.Returns<Task<NodeErrorMessageDto>>(_ => throw new NodeErrorMessageNotFoundException());

		// Act
		Func<Task> act = async () => await _sut.GetNodeErrorMessage(executionId, nodeId, CancellationToken.None);

		// Assert
		act.Should().ThrowAsync<NodeErrorMessageNotFoundException>();
	}
}
