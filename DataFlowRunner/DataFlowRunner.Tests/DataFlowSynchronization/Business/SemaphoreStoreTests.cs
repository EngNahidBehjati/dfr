using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowSynchronization.Business;
using FluentAssertions;

namespace DataFlowRunner.Tests.DataFlowSynchronization.Business;

public sealed class SemaphoreStoreTests
{
	private readonly SemaphoreStore _sut = new();

	[Fact]
	public void GetSemaphore_ShouldCreateSemaphoreWithOneCurrentCount_WhenNotExist()
	{
		// Arrange
		var dataFlowExecutionId = DataFlowExecutionId.New();
		const string action = "action";

		// Act
		SemaphoreSlim actual = _sut.GetSemaphore(dataFlowExecutionId, action);

		// Assert
		actual.CurrentCount.Should().Be(1);
	}

	[Fact]
	public void GetSemaphore_ShouldReturnSameSemaphores_WhenCalledWithSameIdAndAction()
	{
		// Arrange
		var dataFlowExecutionId = DataFlowExecutionId.New();
		const string action = "action";

		// Act
		SemaphoreSlim actual1 = _sut.GetSemaphore(dataFlowExecutionId, action);
		SemaphoreSlim actual2 = _sut.GetSemaphore(dataFlowExecutionId, action);

		// Assert
		actual1.Should().BeSameAs(actual2);
	}

	[Fact]
	public void GetSemaphore_ShouldReturnDifferentSemaphores_WhenCalledWithDifferentActions()
	{
		// Arrange
		var dataFlowExecutionId = DataFlowExecutionId.New();
		const string action1 = "action1";
		const string action2 = "action2";

		// Act
		SemaphoreSlim actual1 = _sut.GetSemaphore(dataFlowExecutionId, action1);
		SemaphoreSlim actual2 = _sut.GetSemaphore(dataFlowExecutionId, action2);

		// Assert
		actual1.Should().NotBe(actual2);
	}

	[Fact]
	public void GetSemaphore_ShouldCreateNewSemaphores_WhenRemoveDataFlowSemaphoresIsCalled()
	{
		// Arrange
		var dataFlowExecutionId = DataFlowExecutionId.New();
		const string action = "action";

		// Act
		SemaphoreSlim semaphoreBeforeRemove = _sut.GetSemaphore(dataFlowExecutionId, action);
		_sut.RemoveDataFlowSemaphores(dataFlowExecutionId);
		SemaphoreSlim semaphoreAfterRemove = _sut.GetSemaphore(dataFlowExecutionId, action);

		// Assert
		semaphoreAfterRemove.Should().NotBeSameAs(semaphoreBeforeRemove);
		semaphoreAfterRemove.CurrentCount.Should().Be(1);
	}
}
