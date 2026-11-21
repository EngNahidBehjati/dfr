using DataFlowRunner.DataFlowDynamicParameters.Business.Store;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using FluentAssertions;

namespace DataFlowRunner.Tests.DataFlowDynamicParameters.Business.Store;

public class NodeDynamicParametersStoreTests
{
	private readonly NodeDynamicParametersStore _sut;

	public NodeDynamicParametersStoreTests()
	{
		_sut = new NodeDynamicParametersStore();
	}

	[Fact]
	public void GetDynamicParameters_ShouldReturnNodeDynamicParameters_WhenNodeIdExist()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());
		var expectedDynamicParameters = new List<DynamicParameter>
		{
			new() { Name = "age", Value = 1 }
		};

		_sut.SetDynamicParameters(nodeId, expectedDynamicParameters);

		// Act
		List<DynamicParameter> actual = _sut.GetDynamicParameters(nodeId);

		// Assert
		actual.Should().BeEquivalentTo(expectedDynamicParameters);
	}

	[Fact]
	public void GetDynamicParameters_ShouldReturnDynamicParametersWithEmptyParameters_WhenNodeIdNotExist()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());
		var expectedDynamicParameters = new List<DynamicParameter>();

		_sut.SetDynamicParameters(nodeId, expectedDynamicParameters);

		// Act
		List<DynamicParameter> actual = _sut.GetDynamicParameters(nodeId);

		// Assert
		actual.Should().BeEquivalentTo(expectedDynamicParameters);
	}

	[Fact]
	public void GetDynamicParameters_ShouldUpdateNodeDynamicParameters_WhenCalledMultipleTimes()
	{
		// Arrange
		var nodeId = new NodeId(Guid.NewGuid());
		var dynamicParameters1 = new List<DynamicParameter>
		{
			new() { Name = "age", Value = 1 }
		};
		var dynamicParameters2 = new List<DynamicParameter>
		{
			new() { Name = "age", Value = 10 },
			new() { Name = "name", Value = "asghar" }
		};

		_sut.SetDynamicParameters(nodeId, dynamicParameters1);
		_sut.SetDynamicParameters(nodeId, dynamicParameters2);

		// Act
		List<DynamicParameter> actual = _sut.GetDynamicParameters(nodeId);

		// Assert
		actual.Should().BeEquivalentTo(dynamicParameters2);
	}
}
