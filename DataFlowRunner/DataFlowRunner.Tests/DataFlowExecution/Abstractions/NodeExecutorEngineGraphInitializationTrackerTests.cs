using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.DataFlowExecution.Models;
using FluentAssertions;

namespace DataFlowRunner.Tests.DataFlowExecution.Abstractions;

public class NodeExecutorEngineGraphInitializationTrackerTests
{
	private readonly NodeExecutorEngineGraphInitializationTracker _sut = new();

	[Fact]
	public void TrackDataFlowInitialization_ShouldMarkExecutionAsInitialized_Whenever()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());

		// Act
		_sut.TrackDataFlowInitialization(executionId);

		// Assert
		_sut.IsDataFlowInitialized(executionId).Should().BeTrue();
	}

	[Fact]
	public void UntrackDataFlowInitialization_ShouldUnmarkExecution_Whenever()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		_sut.TrackDataFlowInitialization(executionId);

		// Act
		_sut.UntrackDataFlowInitialization(executionId);

		// Assert
		_sut.IsDataFlowInitialized(executionId).Should().BeFalse();
	}

	[Fact]
	public void IsDataFlowInitialized_ShouldReturnFalse_WhenExecutionNeverTracked_Whenever()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());

		// Act
		var isInitialized = _sut.IsDataFlowInitialized(executionId);

		// Assert
		isInitialized.Should().BeFalse();
	}
}
