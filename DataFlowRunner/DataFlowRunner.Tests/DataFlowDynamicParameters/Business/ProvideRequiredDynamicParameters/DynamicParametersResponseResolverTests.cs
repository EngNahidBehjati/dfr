using DataFlowRunner.DataFlowDynamicParameters.Abstractions.ProvideRequiredDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Validation;
using DataFlowRunner.DataFlowDynamicParameters.Business.ProvideRequiredDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowDynamicParameters.Business.ProvideRequiredDynamicParameters;

public sealed class DynamicParametersResponseResolverTests
{
	private readonly INodeAncestryParameterValueExtractor _ancestryParameterValueExtractor;
	private readonly IDataFlowParametersValueExtractor _dataFlowParametersValueExtractor;
	private readonly IDynamicParameterTypeValidator _dynamicParameterTypeValidator;
	private readonly DynamicParametersResponseResolver _sut;

	public DynamicParametersResponseResolverTests()
	{
		_ancestryParameterValueExtractor = Substitute.For<INodeAncestryParameterValueExtractor>();
		_dataFlowParametersValueExtractor = Substitute.For<IDataFlowParametersValueExtractor>();
		_dynamicParameterTypeValidator = Substitute.For<IDynamicParameterTypeValidator>();
		_sut = new DynamicParametersResponseResolver(_ancestryParameterValueExtractor, _dataFlowParametersValueExtractor, _dynamicParameterTypeValidator);
	}

	[Fact]
	public void ResolveForNode_ShouldReturnApiDefaultValues_WhenAncestryResolverReturnsNoParameters()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var targetNodeId = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		_ancestryParameterValueExtractor
			.Extract(executionId, targetNodeId, graph, Arg.Is<IReadOnlyCollection<string>>(names => names.Count == 1 && names.Contains("param1")))
			.Returns([]);

		var response = CreateResponse("param1", "INTEGER", false, 42);

		// Act
		List<DynamicParameterDto> actual = _sut.ResolveForNode(executionId, targetNodeId, graph, response);

		// Assert
		actual.Should().ContainSingle().Which.DefaultValue.Should().Be(42);
	}

	[Fact]
	public void ResolveForNode_ShouldUseStoredValue_WhenTypeValidatorReturnsTrue()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var targetNodeId = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		const long storedValue = 99L;
		_ancestryParameterValueExtractor
			.Extract(executionId, targetNodeId, graph, Arg.Is<IReadOnlyCollection<string>>(names => names.Count == 1 && names.Contains("param1")))
			.Returns([new DynamicParameter { Name = "param1", Value = storedValue }]);
		_dynamicParameterTypeValidator.IsConvertible(storedValue, "INTEGER", false).Returns(true);

		var response = CreateResponse("param1", "INTEGER", false, 42);

		// Act
		List<DynamicParameterDto> actual = _sut.ResolveForNode(executionId, targetNodeId, graph, response);

		// Assert
		actual.Should().ContainSingle().Which.DefaultValue.Should().Be(storedValue);
	}

	[Fact]
	public void ResolveForNode_ShouldUseApiDefaultValue_WhenTypeValidatorReturnsFalse()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var targetNodeId = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		const string storedValue = "not-a-number";
		_ancestryParameterValueExtractor
			.Extract(executionId, targetNodeId, graph, Arg.Is<IReadOnlyCollection<string>>(names => names.Count == 1 && names.Contains("param1")))
			.Returns([new DynamicParameter { Name = "param1", Value = storedValue }]);
		_dynamicParameterTypeValidator.IsConvertible(storedValue, "INTEGER", false).Returns(false);

		var response = CreateResponse("param1", "INTEGER", false, 42);

		// Act
		List<DynamicParameterDto> actual = _sut.ResolveForNode(executionId, targetNodeId, graph, response);

		// Assert
		actual.Should().ContainSingle().Which.DefaultValue.Should().Be(42);
	}

	[Fact]
	public void ResolveForNode_ShouldReturnOnlyApiDeclaredParameters_WhenAncestryReturnsExtraParameterNames()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var targetNodeId = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		_ancestryParameterValueExtractor
			.Extract(executionId, targetNodeId, graph, Arg.Is<IReadOnlyCollection<string>>(names => names.Count == 1 && names.Contains("param1")))
			.Returns(
			[
				new DynamicParameter { Name = "param1", Value = 99L },
				new DynamicParameter { Name = "only-in-ancestry", Value = "ignored" }
			]);
		_dynamicParameterTypeValidator.IsConvertible(99L, "INTEGER", false).Returns(true);

		var response = CreateResponse("param1", "INTEGER", false, 42);

		// Act
		List<DynamicParameterDto> actual = _sut.ResolveForNode(executionId, targetNodeId, graph, response);

		// Assert
		actual.Should().ContainSingle().Which.Name.Should().Be("param1");
	}

	[Fact]
	public void ResolveForNode_ShouldUseStoredListValue_WhenTypeValidatorReturnsTrueForListParameter()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var targetNodeId = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		var storedValue = new List<object?> { 1L, 2L };
		_ancestryParameterValueExtractor
			.Extract(executionId, targetNodeId, graph, Arg.Is<IReadOnlyCollection<string>>(names => names.Count == 1 && names.Contains("param1")))
			.Returns([new DynamicParameter { Name = "param1", Value = storedValue }]);
		_dynamicParameterTypeValidator.IsConvertible(storedValue, "INTEGER", true).Returns(true);

		var response = CreateResponse("param1", "INTEGER", isList: true, defaultValue: new List<object?> { 0L });

		// Act
		List<DynamicParameterDto> actual = _sut.ResolveForNode(executionId, targetNodeId, graph, response);

		// Assert
		actual.Should().ContainSingle().Which.DefaultValue.Should().BeEquivalentTo(storedValue);
		_dynamicParameterTypeValidator.Received(1).IsConvertible(storedValue, "INTEGER", true);
	}

	[Fact]
	public void ResolveForFlow_ShouldReturnApiDefaultValues_WhenFlowParametersResolverReturnsEmpty()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		_dataFlowParametersValueExtractor.Extract(executionId, graph).Returns([]);

		var response = CreateResponse("param1", "INTEGER", false, 42);

		// Act
		List<DynamicParameterDto> actual = _sut.ResolveForFlow(executionId, graph, response);

		// Assert
		actual.Should().ContainSingle().Which.DefaultValue.Should().Be(42);
		_dynamicParameterTypeValidator.DidNotReceive().IsConvertible(
			Arg.Any<object?>(), Arg.Any<string>(), Arg.Any<bool>());
	}

	[Fact]
	public void ResolveForFlow_ShouldUseStoredValue_WhenTypeValidatorReturnsTrue()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		const long storedValue = 77L;
		_dataFlowParametersValueExtractor.Extract(executionId, graph)
			.Returns([new DynamicParameter { Name = "param1", Value = storedValue }]);
		_dynamicParameterTypeValidator.IsConvertible(storedValue, "INTEGER", false).Returns(true);

		var response = CreateResponse("param1", "INTEGER", false, 0);

		// Act
		List<DynamicParameterDto> actual = _sut.ResolveForFlow(executionId, graph, response);

		// Assert
		actual.Should().ContainSingle().Which.DefaultValue.Should().Be(storedValue);
		_dynamicParameterTypeValidator.Received(1).IsConvertible(storedValue, "INTEGER", false);
	}

	[Fact]
	public void ResolveForFlow_ShouldUseApiDefaultValue_WhenTypeValidatorReturnsFalse()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		const string storedValue = "not-a-number";
		_dataFlowParametersValueExtractor.Extract(executionId, graph)
			.Returns([new DynamicParameter { Name = "param1", Value = storedValue }]);
		_dynamicParameterTypeValidator.IsConvertible(storedValue, "INTEGER", false).Returns(false);

		var response = CreateResponse("param1", "INTEGER", false, 42);

		// Act
		List<DynamicParameterDto> actual = _sut.ResolveForFlow(executionId, graph, response);

		// Assert
		actual.Should().ContainSingle().Which.DefaultValue.Should().Be(42);
		_dynamicParameterTypeValidator.Received(1).IsConvertible(storedValue, "INTEGER", false);
	}


	private static ResponseDataFlowDynamicParametersDto CreateResponse(
		string parameterName,
		string dataType,
		bool isList,
		object? defaultValue) =>
		new(
		[
			new ResponseNodeDynamicParametersDto(Guid.NewGuid(),
			[
				new ResponseDataFlowDynamicParameterDto("id-1", parameterName, false, dataType, isList, defaultValue)
			])
		]);
}
