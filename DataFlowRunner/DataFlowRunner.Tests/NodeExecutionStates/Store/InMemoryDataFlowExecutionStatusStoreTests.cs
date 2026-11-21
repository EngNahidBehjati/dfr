using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.NodeExecutionStates.Models;
using DataFlowRunner.NodeExecutionStates.Store;
using FluentAssertions;

namespace DataFlowRunner.Tests.NodeExecutionStates.Store;

public sealed class InMemoryDataFlowExecutionStatusStoreTests
{
	private readonly InMemoryDataFlowExecutionStatusStore _sut = new();

	[Fact]
	public void Get_ShouldReturnDefaultStatus_WhenExecutionIdDoesNotExist()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());

		// Act
		DataFlowExecutionStatus result = _sut.Get(executionId);

		// Assert
		result.Failed.Should().BeFalse();
		result.FailureMessage.Should().BeNull();
	}

	[Fact]
	public void Get_ShouldReturnExecutionStatus_WhenExecutionIdExists()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		const string failureMessage = "Something went wrong";
		var status = new DataFlowExecutionStatus(failureMessage);
		_sut.Set(executionId, status);

		// Act
		DataFlowExecutionStatus result = _sut.Get(executionId);

		// Assert
		result.Failed.Should().BeTrue();
		result.FailureMessage.Should().Be(failureMessage);
	}
}
