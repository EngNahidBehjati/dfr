using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.Tests.Common;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecution.Business;

public class DataFlowExecutionContextContainerTests
{
	private readonly DataFlowExecutionContextContainer _sut = new();

	[Fact]
	public void TryGetDataFlowExecutionContext_ShouldReturnFalse_WhenGraphIsMissing()
	{
		// Arrange
		var graphId = new DataFlowExecutionId(Guid.NewGuid());

		// Act
		var result = _sut.TryGetDataFlowExecutionContext(graphId, out DataFlowExecutionContext? graph);

		// Assert
		result.Should().BeFalse();
		graph.Should().BeNull();
	}

	[Fact]
	public void AddOrUpdateDataFlowExecutionContext_ShouldStoreGraphByIdـWhenGraphDoseNotExistsBefore()
	{
		// Arrange
		var dataFlowId = new DataFlowExecutionId(Guid.NewGuid());
		IGraph? graph = Substitute.For<IGraph>();
		var dataFlow = new DataFlowExecutionContext
		{
			Graph = graph,
			DataFlowExecutionId = dataFlowId,
			DataFlowVersion = new DataFlowVersion(0),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		// Act
		_sut.AddOrUpdateDataFlowExecutionContext(dataFlow);
		var found = _sut.TryGetDataFlowExecutionContext(dataFlowId, out DataFlowExecutionContext? storedGraph);

		// Assert
		found.Should().BeTrue();
		storedGraph?.Graph.Should().Be(graph);
	}

	[Fact]
	public void AddOrUpdateDataFlowExecutionContext_ShouldUpdateGraphById_WhenGraphWithSameIdAlreadyExists()
	{
		// Arrange
		var dataFlowId = new DataFlowExecutionId(Guid.NewGuid());
		IGraph? graph = Substitute.For<IGraph>();
		var firstDataFlow = new DataFlowExecutionContext
		{
			Graph = graph,
			DataFlowExecutionId = dataFlowId,
			DataFlowVersion = new DataFlowVersion(0),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		var secondDataFlow = new DataFlowExecutionContext
		{
			Graph = graph,
			DataFlowExecutionId = dataFlowId,
			DataFlowVersion = new DataFlowVersion(2),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};

		// Act
		_sut.AddOrUpdateDataFlowExecutionContext(firstDataFlow);
		_sut.AddOrUpdateDataFlowExecutionContext(secondDataFlow);
		var found = _sut.TryGetDataFlowExecutionContext(dataFlowId, out DataFlowExecutionContext? storedGraph);

		// Assert
		found.Should().BeTrue();
		storedGraph.Should().Be(secondDataFlow);
	}
}
