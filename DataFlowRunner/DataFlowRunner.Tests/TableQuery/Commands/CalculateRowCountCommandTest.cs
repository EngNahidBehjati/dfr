using System.Text.Json;
using DataFlowRunner.TableQuery.Commands;
using FluentAssertions;

namespace DataFlowRunner.Tests.TableQuery.Commands;

public class CalculateRowCountCommandTest
{
	[Fact]
	public void GetLogParameters_ShouldReturnExpectedLogParameters_WhenCalled()
	{
		// Arrange
		var nodeId = Guid.NewGuid();
		var userId = Guid.NewGuid();
		var graphExecutionId = Guid.NewGuid();
		var oDataQuery = new Dictionary<string, string>()
		{
			["key"] = "value"
		};
		var outputPortId = Guid.NewGuid();

		var expected = new Dictionary<string, object>()
		{
			["GraphExecutionId"] = graphExecutionId,
			["UserId"] = userId,
			["NodeId"] = nodeId,
			["CommandType"] = nameof(CalculateRowCountCommand),
			["OutputPortId"] = outputPortId,
			["ODataQuery"] = JsonSerializer.Serialize(oDataQuery)
		};
		var sut = new CalculateRowCountCommand()
		{
			UserId = userId,
			NodeId = nodeId,
			GraphExecutionId = graphExecutionId,
			ODataQuery = oDataQuery,
			OutputPortId = outputPortId
		};
		// Act
		IReadOnlyDictionary<string, object> actual = sut.GetLogParameters();

		// Assert
		actual.Should().BeEquivalentTo(expected);
	}
}
