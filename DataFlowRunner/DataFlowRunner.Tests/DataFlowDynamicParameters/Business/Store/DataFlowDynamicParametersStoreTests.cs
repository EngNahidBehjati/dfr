using DataFlowRunner.DataFlowDynamicParameters.Business.Store;
using DataFlowRunner.DataFlowDynamicParameters.Exceptions;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using FluentAssertions;

namespace DataFlowRunner.Tests.DataFlowDynamicParameters.Business.Store;

public class DataFlowDynamicParametersStoreTests
{
	private readonly DataFlowDynamicParametersStore _sut;

	public DataFlowDynamicParametersStoreTests()
	{
		_sut = new DataFlowDynamicParametersStore(LanguageDictionaryProvider.Instance);
	}

	[Fact]
	public void GetDynamicParameters_ShouldThrowDynamicParametersNotSetException_WhenExecutionIdDoesNotExists()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());

		// Act
		Func<List<DynamicParameter>> act =  () => _sut.GetDynamicParameters(dataFlowExecutionId, nodeId);

		// Assert
		act.Should().Throw<DynamicParametersNotSetException>();
	}

	[Fact]
	public void GetDynamicParameters_ShouldReturnNodeDynamicParameters_WhenExecutionIdExists()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		var expectedDynamicParameters = new List<DynamicParameter>
		{
			new() { Name = "age", Value = 1 }
		};

		_sut.SetDynamicParameters(dataFlowExecutionId, [nodeId], expectedDynamicParameters);

		// Act
		List<DynamicParameter> actual = _sut.GetDynamicParameters(dataFlowExecutionId, nodeId);

		// Assert
		actual.Should().BeEquivalentTo(expectedDynamicParameters);
	}

	[Fact]
	public void GetDynamicParameters_ShouldAddOrUpdateDynamicParametersValues_WhenSetMultipleTimes()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		var dynamicParameters1 = new List<DynamicParameter>
		{
			new() { Name = "age", Value = 1 },
			new() { Name = "count", Value = 2 }
		};
		var dynamicParameters2 = new List<DynamicParameter>
		{
			new() { Name = "age", Value = 2 },
			new() { Name = "name", Value = "name1" }
		};

		_sut.SetDynamicParameters(dataFlowExecutionId, [nodeId], dynamicParameters1);
		_sut.SetDynamicParameters(dataFlowExecutionId, [nodeId], dynamicParameters2);

		// Act
		List<DynamicParameter> actual = _sut.GetDynamicParameters(dataFlowExecutionId, nodeId);

		// Assert
		actual.Should().BeEquivalentTo(dynamicParameters2);
	}
}
