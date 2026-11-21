using DataFlowRunner.Common.Exceptions.NotFound;
using DataFlowRunner.DataFlowNodeConfigs.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using FluentAssertions;

namespace DataFlowRunner.Tests.DataFlowNodeConfigs.Models;

public class DataFlowExecutionNodeConfigsTests
{
	private readonly DataFlowExecutionNodeConfigs _sut;

	public DataFlowExecutionNodeConfigsTests()
	{
		_sut = new DataFlowExecutionNodeConfigs(LanguageDictionaryProvider.Instance);
	}

	[Fact]
	public void GetConfig_ShouldReturnConfig_WhenNodeConfigExists()
	{
		// Arrange
		var nodeId = new NodeId();
		var nodeConfig = new NodeConfig() { Content = "NodeConfigContent", Version = new NodeVersion("31") };
		_sut.SetConfig(nodeId, nodeConfig);

		// Act
		NodeConfig result = _sut.GetConfig(nodeId);

		// Assert
		result.Should().Be(nodeConfig);
	}

	[Fact]
	public void GetConfig_ShouldThrowNodeNotFoundException_WhenNodeConfigDoesNotExists()
	{
		// Arrange
		var nodeId = new NodeId();

		// Act
		Func<NodeConfig> act = () => _sut.GetConfig(nodeId);

		// Assert
		act.Should().Throw<NodeNotFoundException>();
	}
}
