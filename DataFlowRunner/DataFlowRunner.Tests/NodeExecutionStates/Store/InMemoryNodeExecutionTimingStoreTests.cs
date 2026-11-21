using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Exceptions;
using DataFlowRunner.NodeExecutionStates.Models;
using DataFlowRunner.NodeExecutionStates.Store;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.NodeExecutionStates.Store;

public sealed class InMemoryNodeExecutionTimingStoreTests
{
	private readonly TimeProvider _timeProvider;
	private readonly InMemoryNodeExecutionTimingStore _sut;

    public InMemoryNodeExecutionTimingStoreTests()
    {
	    _timeProvider = Substitute.For<TimeProvider>();
	    _sut = new InMemoryNodeExecutionTimingStore(_timeProvider, LanguageDictionaryProvider.Instance);
    }

    [Fact]
    public void SetNodeExecutionStartTime_ShouldSetStartTime_WhenCalled()
    {
        // Arrange
        var executionId = new DataFlowExecutionId(Guid.NewGuid());
        var nodeId = new NodeId(Guid.NewGuid());
        var startTime = DateTimeOffset.Parse("2026-01-01T00:00:00.0000000Z");
        var expected = new NodeExecutionTiming { StartedAt = startTime };

        _timeProvider.GetUtcNow().Returns(startTime);

        // Act
        _sut.SetNodeExecutionStartTime(executionId, nodeId);

        // Assert
        NodeExecutionTiming actual = _sut.GetNodeExecutionTime(executionId, nodeId);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void SetNodeExecutionEndTime_ShouldSetEndTime_WhenStartTimeIsSet()
    {
        // Arrange
        var executionId = new DataFlowExecutionId(Guid.NewGuid());
        var nodeId = new NodeId(Guid.NewGuid());
        var startTime = DateTimeOffset.Parse("2026-01-01T00:00:00.0000000Z");
        var endTime = DateTimeOffset.Parse("2026-01-02T00:00:00.0000000Z");
        var expected = new NodeExecutionTiming { StartedAt = startTime, CompletedAt = endTime };

        _timeProvider.GetUtcNow().Returns(startTime);
        _sut.SetNodeExecutionStartTime(executionId, nodeId);
        _timeProvider.GetUtcNow().Returns(endTime);

        // Act
        _sut.SetNodeExecutionEndTime(executionId, nodeId);

        // Assert
        NodeExecutionTiming actual = _sut.GetNodeExecutionTime(executionId, nodeId);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void SetNodeExecutionEndTime_ShouldThrowNodeExecutionStartTimeNotSetException_WhenStartTimeNotSet()
    {
	    // Arrange
	    var executionId = new DataFlowExecutionId(Guid.NewGuid());
	    var nodeId = new NodeId(Guid.NewGuid());
	    var endTime = DateTimeOffset.Parse("2026-01-02T00:00:00.0000000Z");

	    _timeProvider.GetUtcNow().Returns(endTime);

	    // Act
	    Action act = () => _sut.SetNodeExecutionEndTime(executionId, nodeId);

	    // Assert
	    act.Should().Throw<NodeExecutionStartTimeNotSetException>();
    }

    [Fact]
    public void GetNodeExecutionTime_ShouldReturnEmptyObject_WhenExecutionIdDoesNotExist()
    {
	    // Arrange
	    var executionId = new DataFlowExecutionId(Guid.NewGuid());
	    var nodeId = new NodeId(Guid.NewGuid());
	    var expected = new NodeExecutionTiming();

	    // Act
	    NodeExecutionTiming actual = _sut.GetNodeExecutionTime(executionId, nodeId);

	    // Assert
	    actual.Should().BeEquivalentTo(expected);
    }
}
