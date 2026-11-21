using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using FluentAssertions;

namespace DataFlowRunner.Tests.DataFlowExecutionEvents.ExecutionCommands;

public class CancelNodeCommandTest
{
	[Fact]
	public void GetLogParameters_ShouldReturnExpectedLogParameters_WhenCalled()
	{
		// Arrange
		var nodeId = Guid.NewGuid();
		var userId = Guid.NewGuid();
		var graphExecutionId = Guid.NewGuid();

		var sut = new CancelNodeCommand()
		{
			NodeId = nodeId,
			UserId = userId,
			GraphExecutionId = graphExecutionId
		};
		var expected = new Dictionary<string, object>()
		{
			["GraphExecutionId"] = graphExecutionId,
			["UserId"] = userId,
			["NodeId"] = nodeId,
			["CommandType"] = "CancelNodeCommand"
		};

		// Act
		IReadOnlyDictionary<string, object> actual = sut.GetLogParameters();

		// Assert
		actual.Should().BeEquivalentTo(expected);
	}
}
