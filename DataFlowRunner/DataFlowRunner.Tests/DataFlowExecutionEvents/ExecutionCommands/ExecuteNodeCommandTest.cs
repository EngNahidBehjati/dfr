using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using FluentAssertions;

namespace DataFlowRunner.Tests.DataFlowExecutionEvents.ExecutionCommands;

public class ExecuteNodeCommandTest
{
	[Fact]
	public void GetLogParameters_ShouldReturnExpectedLogParameters_WhenCalled()
	{
		// Arrange
		var nodeId = Guid.NewGuid();
		var userId = Guid.NewGuid();
		var graphExecutionId = Guid.NewGuid();

		var sut = new ExecuteNodeCommand()
		{
			UserId = userId,
			NodeId = nodeId,
			GraphExecutionId = graphExecutionId,
			Config = "config",
			DynamicParameters = [],
			PortConnections = []
		};
		var expected = new Dictionary<string, object>()
		{
			["GraphExecutionId"] = graphExecutionId,
			["UserId"] = userId,
			["NodeId"] = nodeId,
			["CommandType"] = "ExecuteNodeCommand"
		};

		// Act
		IReadOnlyDictionary<string, object> actual = sut.GetLogParameters();

		// Assert
		actual.Should().BeEquivalentTo(expected);
	}
}
