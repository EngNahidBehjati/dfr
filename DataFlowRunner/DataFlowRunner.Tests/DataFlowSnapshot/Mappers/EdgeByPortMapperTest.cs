using DataFlowRunner.DataFlowSnapshot.Mappers;
using DataFlowRunner.DataFlowSnapshot.Models;
using DataFlowRunner.NodePortConnections.Models;
using FluentAssertions;

namespace DataFlowRunner.Tests.DataFlowSnapshot.Mappers;

public class EdgeByPortMapperTests
{
	private readonly EdgeByPortMapper _sut = new();

	[Fact]
	public void MapToEdgeByPorts_ShouldMapAllFieldsCorrectly_Whenever()
	{
		// Arrange
		var edge1 = new EdgeDto
		(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid()
		);
		var edge2 = new EdgeDto
		(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid()
		);
		var input = new List<EdgeDto> { edge1, edge2 };

		// Act
		IReadOnlyList<EdgeByPort> result = _sut.MapToEdgeByPorts(input);

		// Assert

		result.Should().HaveCount(2);
		result[0].Id.Value.Should().Be(edge1.EdgeId);
		result[0].FromPortId.Value.Should().Be(edge1.FromPort);
		result[0].ToPortId.Value.Should().Be(edge1.ToPort);

		result[1].Id.Value.Should().Be(edge2.EdgeId);
		result[1].FromPortId.Value.Should().Be(edge2.FromPort);
		result[1].ToPortId.Value.Should().Be(edge2.ToPort);
	}
}
