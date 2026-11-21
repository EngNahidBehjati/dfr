using DataFlowRunner.NodeExecutionStates.Business;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.NodeExecutionStates.Business;

public class NodeExecutionDurationResolverTests
{
	private readonly TimeProvider _timeProvider;
	private readonly NodeExecutionDurationResolver _sut;

	public NodeExecutionDurationResolverTests()
	{
		_timeProvider = Substitute.For<TimeProvider>();
		_sut = new NodeExecutionDurationResolver(_timeProvider);
	}

	[Fact]
	public void ResolveDurationMilliseconds_ShouldReturnNull_WhenStartTimeIsNull()
	{
		// Arrange
		var nodeExecutionTiming = new NodeExecutionTiming
		{
			StartedAt = null,
			CompletedAt = null
		};

		// Act
		var actual = _sut.ResolveDurationMilliseconds(nodeExecutionTiming);

		// Assert
		actual.Should().BeNull();
	}

	[Fact]
	public void ResolveDurationMilliseconds_ShouldReturnDurationFromStartTimeToNow_WhenStartTimeIsSetAndCompletedTimeIsNull()
	{
		// Arrange
		var nodeExecutionTiming = new NodeExecutionTiming
		{
			StartedAt = DateTimeOffset.Parse("2026-01-02T00:00:00.0000000Z"),
			CompletedAt = null
		};
		const int expected = 7100;
		_timeProvider.GetUtcNow().Returns(DateTimeOffset.Parse("2026-01-02T00:00:07.1000000Z"));

		// Act
		var actual = _sut.ResolveDurationMilliseconds(nodeExecutionTiming);

		// Assert
		actual.Should().Be(expected);
	}

	[Fact]
	public void ResolveDurationMilliseconds_ShouldReturnDurationFromStartTimeToCompletedTime_WhenStartAndCompletedTimeIsSet()
	{
		// Arrange
		var nodeExecutionTiming = new NodeExecutionTiming
		{
			StartedAt = DateTimeOffset.Parse("2026-01-02T00:00:00.0000000Z"),
			CompletedAt = DateTimeOffset.Parse("2026-01-02T00:00:08.1000000Z")
		};
		const int expected = 8100;

		// Act
		var actual = _sut.ResolveDurationMilliseconds(nodeExecutionTiming);

		// Assert
		actual.Should().Be(expected);
	}
}
