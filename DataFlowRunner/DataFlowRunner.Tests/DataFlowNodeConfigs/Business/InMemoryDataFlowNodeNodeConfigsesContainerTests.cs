using DataFlowRunner.Common.Exceptions.NotFound;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowNodeConfigs.Business;
using DataFlowRunner.DataFlowNodeConfigs.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using FluentAssertions;

namespace DataFlowRunner.Tests.DataFlowNodeConfigs.Business;

public class InMemoryDataFlowNodeConfigsStoreTests
{
	private readonly InMemoryDataFlowNodeConfigsStore _sut;

	public InMemoryDataFlowNodeConfigsStoreTests()
	{
		_sut = new InMemoryDataFlowNodeConfigsStore(LanguageDictionaryProvider.Instance);
	}

	[Fact]
	public void GetNodeConfig_ShouldReturnConfig_WhenConfigExists()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		var config = new NodeConfig() { Content = "test", Version = new NodeVersion("144") };
		_sut.SetNodeConfig(dataFlowExecutionId, nodeId, config);

		// Act
		NodeConfig result = _sut.GetNodeConfig(dataFlowExecutionId, nodeId);

		// Assert
		result.Should().Be(config);
	}

	[Fact]
	public void GetNodeConfig_ShouldThrowNodeNotFoundException_WhenConfigDoesNotExistButDataFlowExists()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		var tempConfig = new NodeConfig() { Content = "test", Version = new NodeVersion("144") };
		_sut.SetNodeConfig(dataFlowExecutionId, new NodeId(Guid.NewGuid()), tempConfig);

		// Act
		Func<NodeConfig> act = () => _sut.GetNodeConfig(dataFlowExecutionId, nodeId);

		// Assert
		act.Should().Throw<NodeNotFoundException>();
	}

	[Fact]
	public void GetNodeConfig_ShouldThrowDataFlowExecutionNotFoundException_WhenDataFlowDoesNotExist()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());

		// Act
		Func<NodeConfig> act = () => _sut.GetNodeConfig(dataFlowExecutionId, nodeId);

		// Assert
		act.Should().Throw<DataFlowExecutionNotFoundException>();
	}

	[Fact]
	public void GetNodeConfig_ShouldGetLastConfig_WhenConfigSetMultipleTimes()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		var config1 = new NodeConfig() { Content = "test", Version = new NodeVersion("144") };
		var config2 = new NodeConfig() { Content = "test2", Version = new NodeVersion("145") };

		// Act
		_sut.SetNodeConfig(dataFlowExecutionId, nodeId, config1);
		_sut.SetNodeConfig(dataFlowExecutionId, nodeId, config2);
		NodeConfig result = _sut.GetNodeConfig(dataFlowExecutionId, nodeId);

		// Assert
		result.Should().Be(config2);
	}
}
