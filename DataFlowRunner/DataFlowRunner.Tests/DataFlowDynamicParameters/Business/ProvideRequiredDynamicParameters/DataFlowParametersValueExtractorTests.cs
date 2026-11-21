using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;
using DataFlowRunner.DataFlowDynamicParameters.Business.ProvideRequiredDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowDynamicParameters.Business.ProvideRequiredDynamicParameters;

public sealed class DataFlowParametersValueExtractorTests
{
	private readonly IDataFlowDynamicParametersStore _dynamicParametersStore;
	private readonly IDataFlowExecutionSequenceStore _dataFlowExecutionSequenceStore;
	private readonly DataFlowParametersValueExtractor _sut;

	public DataFlowParametersValueExtractorTests()
	{
		_dynamicParametersStore = Substitute.For<IDataFlowDynamicParametersStore>();
		_dataFlowExecutionSequenceStore = Substitute.For<IDataFlowExecutionSequenceStore>();
		_sut = new DataFlowParametersValueExtractor(_dynamicParametersStore, _dataFlowExecutionSequenceStore);
	}

	[Fact]
	public void Extract_ShouldReturnEmpty_WhenGraphHasNoStoredParameters()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		graph.GetAllNodes().Returns([new Node { Id = nodeId }]);
		_dynamicParametersStore
			.TryGetDynamicParameters(executionId, nodeId, out Arg.Any<IReadOnlyCollection<DynamicParameter>>())
			.Returns(callInfo =>
			{
				callInfo[2] = Array.Empty<DynamicParameter>();
				return false;
			});

		// Act
		IReadOnlyCollection<DynamicParameter> actual =
			_sut.Extract(executionId, graph);

		// Assert
		actual.Should().BeEmpty();
	}

	[Fact]
	public void Extract_ShouldPreferParameterFromHigherRunSequence_WhenTwoNodesDefineSameParameterName()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		graph.GetAllNodes().Returns([new Node { Id = nodeA }, new Node { Id = nodeB }]);

		_dynamicParametersStore
			.TryGetDynamicParameters(executionId, nodeA, out Arg.Any<IReadOnlyCollection<DynamicParameter>>())
			.Returns(callInfo =>
			{
				callInfo[2] = new[] { new DynamicParameter { Name = "param1", Value = 10 } };
				return true;
			});
		_dynamicParametersStore
			.TryGetDynamicParameters(executionId, nodeB, out Arg.Any<IReadOnlyCollection<DynamicParameter>>())
			.Returns(callInfo =>
			{
				callInfo[2] = new[] { new DynamicParameter { Name = "param1", Value = 20 } };
				return true;
			});
		_dataFlowExecutionSequenceStore.GetNodeRunSequence(executionId, nodeA).Returns(1);
		_dataFlowExecutionSequenceStore.GetNodeRunSequence(executionId, nodeB).Returns(2);

		// Act
		IReadOnlyCollection<DynamicParameter> actual =
			_sut.Extract(executionId, graph);

		// Assert
		actual.Should().ContainSingle(p => p.Name == "param1" && Equals(p.Value, 20));
	}

	[Fact]
	public void Extract_ShouldReturnSameParameterValue_WhenNodesShareRunSequence()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeA = new NodeId(Guid.NewGuid());
		var nodeB = new NodeId(Guid.NewGuid());
		IReadOnlyGraph graph = Substitute.For<IReadOnlyGraph>();
		graph.GetAllNodes().Returns([new Node { Id = nodeA }, new Node { Id = nodeB }]);

		const int sharedValue = 42;
		_dynamicParametersStore
			.TryGetDynamicParameters(executionId, nodeA, out Arg.Any<IReadOnlyCollection<DynamicParameter>>())
			.Returns(callInfo =>
			{
				callInfo[2] = new[] { new DynamicParameter { Name = "param1", Value = sharedValue } };
				return true;
			});
		_dynamicParametersStore
			.TryGetDynamicParameters(executionId, nodeB, out Arg.Any<IReadOnlyCollection<DynamicParameter>>())
			.Returns(callInfo =>
			{
				callInfo[2] = new[] { new DynamicParameter { Name = "param1", Value = sharedValue } };
				return true;
			});
		_dataFlowExecutionSequenceStore.GetNodeRunSequence(executionId, nodeA).Returns(1);
		_dataFlowExecutionSequenceStore.GetNodeRunSequence(executionId, nodeB).Returns(1);

		// Act
		IReadOnlyCollection<DynamicParameter> actual =
			_sut.Extract(executionId, graph);

		// Assert
		actual.Should().ContainSingle(p => p.Name == "param1" && Equals(p.Value, sharedValue));
	}
}
