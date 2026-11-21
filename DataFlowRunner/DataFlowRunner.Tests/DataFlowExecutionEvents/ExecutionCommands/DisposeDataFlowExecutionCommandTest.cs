using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using FluentAssertions;

namespace DataFlowRunner.Tests.DataFlowExecutionEvents.ExecutionCommands;

public class DisposeDataFlowExecutionCommandTest
{
	[Fact]
	public void GetLogParameters_ShouldReturnExpectedLogParameters_WhenCalled()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var graphExecutionId = Guid.NewGuid();

		var sut = new DisposeDataFlowExecutionCommand()
		{
			UserId = userId,
			GraphExecutionId = graphExecutionId
		};
		var expected = new Dictionary<string, object>()
		{
			["GraphExecutionId"] = graphExecutionId,
			["UserId"] = userId,
			["CommandType"] = "DisposeDataFlowExecutionCommand"
		};

		// Act
		IReadOnlyDictionary<string, object> actual = sut.GetLogParameters();

		// Assert
		actual.Should().BeEquivalentTo(expected);
	}
}
