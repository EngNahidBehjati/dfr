using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using FluentAssertions;

namespace DataFlowRunner.Tests.DataFlowExecutionEvents.ExecutionCommands;

public class InitializeDataFlowExecutionCommandTest
{
	[Fact]
	public void GetLogParameters_ShouldReturnExpectedLogParameters_WhenCalled()
	{
		// Arrange
		var dataFlowId = Guid.NewGuid();
		var userId = Guid.NewGuid();
		var graphExecutionId = Guid.NewGuid();

		var sut = new InitializeDataFlowExecutionCommand()
		{
			UserId = userId,
			DataFlowId = dataFlowId,
			GraphExecutionId = graphExecutionId,
			ProcessorInfo = new ProcessorInfo()
			{
				ProcessorType = "type"
			}
		};
		var expected = new Dictionary<string, object>()
		{
			["GraphExecutionId"] = graphExecutionId,
			["UserId"] = userId,
			["DataFlowId"] = dataFlowId,
			["CommandType"] = "InitializeDataFlowExecutionCommand"
		};

		// Act
		IReadOnlyDictionary<string, object> actual = sut.GetLogParameters();

		// Assert
		actual.Should().BeEquivalentTo(expected);
	}
}
