using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Exceptions;
using DataFlowRunner.NodeExecutionStates.Models;
using DataFlowRunner.NodeExecutionStates.Store;
using FluentAssertions;

namespace DataFlowRunner.Tests.NodeExecutionStates.Store;

public sealed class NodeExecutionTimingContainerTests
{
	private readonly NodeExecutionTimingContainer _sut;

	public NodeExecutionTimingContainerTests()
	{
		_sut = new NodeExecutionTimingContainer(LanguageDictionaryProvider.Instance);
	}

    [Fact]
    public void SetNodeExecutionStartTime_ShouldSetStartTime_WhenNodeIsNew()
    {
        // Arrange
        var nodeId = new NodeId(Guid.NewGuid());
        var startTime = DateTimeOffset.Parse("2026-01-01T00:00:00.0000000Z");
        var expected = new NodeExecutionTiming { StartedAt = startTime };

        // Act
        _sut.SetNodeExecutionStartTime(nodeId, startTime);

        // Assert
        NodeExecutionTiming actual = _sut.GetNodeExecutionTime(nodeId);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void SetNodeExecutionStartTime_ShouldUpdateStartTimeAndResetEndTimeToNull_WhenNodeExists()
    {
	    // Arrange
	    var nodeId = new NodeId(Guid.NewGuid());
	    var initialStartTime = DateTimeOffset.Parse("2026-01-01T00:00:00.0000000Z");
	    var initialEndTime = DateTimeOffset.Parse("2026-01-01T02:00:00.0000000Z");
	    var updatedStartTime = DateTimeOffset.Parse("2026-01-02T00:00:00.0000000Z");
	    var expected = new NodeExecutionTiming { StartedAt = updatedStartTime };

	    _sut.SetNodeExecutionStartTime(nodeId, initialStartTime);
	    _sut.SetNodeExecutionEndTime(nodeId, initialEndTime);

	    // Act
	    _sut.SetNodeExecutionStartTime(nodeId, updatedStartTime);

	    // Assert
	    NodeExecutionTiming actual = _sut.GetNodeExecutionTime(nodeId);
	    actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void SetNodeExecutionEndTime_ShouldSetEndTime_WhenStartTimeIsSet()
    {
        // Arrange
        var nodeId = new NodeId(Guid.NewGuid());
        var startTime = DateTimeOffset.Parse("2026-01-01T00:00:00.0000000Z");
        var endTime = DateTimeOffset.Parse("2026-01-02T00:00:00.0000000Z");
        var expected = new NodeExecutionTiming { StartedAt = startTime, CompletedAt = endTime };
        _sut.SetNodeExecutionStartTime(nodeId, startTime);

        // Act
        _sut.SetNodeExecutionEndTime(nodeId, endTime);

        // Assert
        NodeExecutionTiming actual = _sut.GetNodeExecutionTime(nodeId);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void SetNodeExecutionEndTime_ShouldThrowNodeExecutionStartTimeNotSetException_WhenStartTimeNotSet()
    {
	    // Arrange
	    var nodeId = new NodeId(Guid.NewGuid());
	    var endTime = DateTimeOffset.Parse("2026-01-02T00:00:00.0000000Z");

	    // Act
	    Action act = () => _sut.SetNodeExecutionEndTime(nodeId, endTime);

	    // Assert
	    act.Should().Throw<NodeExecutionStartTimeNotSetException>();
    }

    [Fact]
    public void GetNodeExecutionTime_ShouldReturnNodeExecutionTime_WhenNodeExists()
    {
	    // Arrange
	    var nodeId = new NodeId(Guid.NewGuid());
	    var startTime = DateTimeOffset.Parse("2026-01-01T00:00:00.0000000Z");
	    var endTime = DateTimeOffset.Parse("2026-01-02T00:00:00.0000000Z");

	    var expected = new NodeExecutionTiming { StartedAt = startTime, CompletedAt = endTime };

	    _sut.SetNodeExecutionStartTime(nodeId, startTime);
	    _sut.SetNodeExecutionEndTime(nodeId, endTime);

	    // Act
	    NodeExecutionTiming actual = _sut.GetNodeExecutionTime(nodeId);

	    // Assert
	    actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetNodeExecutionTime_ShouldReturnEmptyObject_WhenNodeDoesNotExist()
    {
        // Arrange
        var nodeId = new NodeId(Guid.NewGuid());
        var expected = new NodeExecutionTiming();

        // Act
        NodeExecutionTiming actual = _sut.GetNodeExecutionTime(nodeId);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }
}
