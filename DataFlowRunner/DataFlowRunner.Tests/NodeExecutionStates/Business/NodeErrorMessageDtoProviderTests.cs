using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Business;
using DataFlowRunner.NodeExecutionStates.DataTransferObjects;
using DataFlowRunner.NodeExecutionStates.Exceptions;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.NodeExecutionStates.Business;

public class NodeErrorMessageDtoProviderTests
{
	private readonly INodeErrorMessageMapper _nodeErrorMessageMapper;
	private readonly INodeErrorMessageStore _nodeErrorMessageStore;
	private readonly NodeErrorMessageDtoProvider _sut;

	public NodeErrorMessageDtoProviderTests()
	{
		_nodeErrorMessageMapper = Substitute.For<INodeErrorMessageMapper>();
		_nodeErrorMessageStore = Substitute.For<INodeErrorMessageStore>();
		_sut = new NodeErrorMessageDtoProvider(_nodeErrorMessageMapper, _nodeErrorMessageStore);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenMapperIsNull()
	{
		// Arrange
		INodeErrorMessageMapper mapper = null!;
		INodeErrorMessageStore store = Substitute.For<INodeErrorMessageStore>();

		// Act
		Action act = () => _ = new NodeErrorMessageDtoProvider(mapper, store);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("nodeErrorMessageMapper");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenStoreIsNull()
	{
		// Arrange
		INodeErrorMessageMapper mapper = Substitute.For<INodeErrorMessageMapper>();
		INodeErrorMessageStore store = null!;

		// Act
		Action act = () => _ = new NodeErrorMessageDtoProvider(mapper, store);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("nodeErrorMessageStore");
	}

	[Fact]
	public async Task GetNodeErrorMessageAsync_ShouldReturnMappedDto_WhenMessageExists()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var message = new NodeErrorMessage { Message = "failure", StackTrace = "stack trace" };
		var dto = new NodeErrorMessageDto { ErrorMessage = "failure", StackTrace = "stack trace" };
		_nodeErrorMessageStore.GetNodeErrorMessage(executionId, nodeId).Returns(message);
		_nodeErrorMessageMapper.Map(message).Returns(dto);

		// Act
		NodeErrorMessageDto result = await _sut.GetNodeErrorMessageAsync(executionId, nodeId, CancellationToken.None);

		// Assert
		result.Should().Be(dto);
	}

	[Fact]
	public void GetNodeErrorMessageAsync_ShouldThrowNodeErrorMessageNotFoundException_WhenNodeErrorMessageNotFound()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		_nodeErrorMessageStore.GetNodeErrorMessage(executionId, nodeId)
			.Returns(_ => throw new NodeErrorMessageNotFoundException());

		// Act
		Func<Task> act = async () => await _sut.GetNodeErrorMessageAsync(executionId, nodeId, CancellationToken.None);

		// Assert
		act.Should().ThrowAsync<NodeErrorMessageNotFoundException>();
	}
}

