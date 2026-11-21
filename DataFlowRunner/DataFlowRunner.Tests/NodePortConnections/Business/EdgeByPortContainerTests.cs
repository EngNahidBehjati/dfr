using DataFlowRunner.Common.Exceptions.NotFound;
using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.GraphCore.Models.PortModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodePortConnections.Business;
using DataFlowRunner.NodePortConnections.Models;
using FluentAssertions;

namespace DataFlowRunner.Tests.NodePortConnections.Business;

public class EdgeByPortContainerTests
{
	private readonly EdgeByPortContainer _sut;

	public EdgeByPortContainerTests()
	{
		_sut = new EdgeByPortContainer(LanguageDictionaryProvider.Instance);
	}

	[Fact]
	public void AddEdgeByPort_ShouldStoreEdgeByPort_WhenEdgeByPortDoesNotExistsBefore()
	{
		// Arrange
		var edgeId = new EdgeId(Guid.NewGuid());
		var toPortId = new InputPortId(Guid.NewGuid());
		var fromPortId = new OutputPortId(Guid.NewGuid());
		var port = new EdgeByPort { ToPortId = toPortId, FromPortId = fromPortId, Id = edgeId };

		// Act
		_sut.TryAddEdgeByPort(port);

		// Assert
		EdgeByPort result = _sut.GetEdgeByPort(edgeId);
		result.ToPortId.Should().Be(toPortId);
		result.FromPortId.Should().Be(fromPortId);
	}

	[Fact]
	public void GetEdgeByPort_ShouldThrowEdgeNotFoundException_WhenEdgeIsMissing()
	{
		// Arrange
		var edgeId = new EdgeId(Guid.NewGuid());

		// Act
		Action act = () => _sut.GetEdgeByPort(edgeId);

		// Assert
		act.Should().Throw<EdgeNotFoundException>();
	}
}
