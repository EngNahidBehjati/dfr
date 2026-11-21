using System.Text.Json;
using DataFlowRunner.TableQuery.Commands;
using FluentAssertions;

namespace DataFlowRunner.Tests.TableQuery.Commands;

public class ExecuteTableQueryCommandTest
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
		var includeSchema = false;

		var expected = new Dictionary<string, object>()
		{
			["GraphExecutionId"] = graphExecutionId,
			["UserId"] = userId,
			["NodeId"] = nodeId,
			["CommandType"] = "ExecuteTableQueryCommand",
			["OutputPortId"] = outputPortId,
			["IncludeSchema"] = includeSchema,
			["ODataQuery"] = JsonSerializer.Serialize(oDataQuery)
		};
		var sut = new ExecuteTableQueryCommand()
		{
			UserId = userId,
			NodeId = nodeId,
			GraphExecutionId = graphExecutionId,
			ODataQuery = oDataQuery,
			OutputPortId = outputPortId,
			IncludeSchema = includeSchema
		};
		// Act
		IReadOnlyDictionary<string, object> actual = sut.GetLogParameters();

		// Assert
		actual.Should().BeEquivalentTo(expected);
	}
}
