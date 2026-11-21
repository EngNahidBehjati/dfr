using DataFlowRunner.DataFlowDynamicParameters.Abstractions;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Tests.Common;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecution.Business;

public sealed class DynamicParameterMismatchNodeInvalidatorTests
{
	private readonly IDataFlowDynamicParametersStore _dataFlowDynamicParametersStore;
	private readonly IDataFlowInvalidator _dataFlowInvalidator;
	private readonly DynamicParameterMismatchNodeInvalidator _sut;

	public DynamicParameterMismatchNodeInvalidatorTests()
	{
		_dataFlowDynamicParametersStore = Substitute.For<IDataFlowDynamicParametersStore>();
		_dataFlowInvalidator = Substitute.For<IDataFlowInvalidator>();
		_sut = new DynamicParameterMismatchNodeInvalidator(_dataFlowDynamicParametersStore, _dataFlowInvalidator);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowDynamicParametersStoreIsNull()
	{
		// Act
		Action act = () => _ = new DynamicParameterMismatchNodeInvalidator(null!, _dataFlowInvalidator);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("dataFlowDynamicParametersStore");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowInvalidatorIsNull()
	{
		// Act
		Action act = () => _ = new DynamicParameterMismatchNodeInvalidator(_dataFlowDynamicParametersStore, null!);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("dataFlowInvalidator");
	}

	[Fact]
	public async Task InvalidateNodesOnParameterMismatchAsync_ShouldInvalidateNode_WhenMatchingStoredParameterHasDifferentValue()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		var dataFlowExecutionContext = CreateContext(executionId);

		IReadOnlyGraph upstreamSubgraph = Substitute.For<IReadOnlyGraph>();
		upstreamSubgraph.GetAllNodes().Returns(new List<Node> { new() { Id = nodeId } });

		List<DynamicParameter> providedDynamicParameters = [new() { Name = "age", Value = 30 }];

		IReadOnlyCollection<DynamicParameter> storedParams = [new DynamicParameter { Name = "age", Value = 10 }];
		_dataFlowDynamicParametersStore
			.TryGetDynamicParameters(executionId, nodeId, out Arg.Any<IReadOnlyCollection<DynamicParameter>>())
			.Returns(callInfo =>
			{
				callInfo[2] = storedParams;
				return true;
			});

		// Act
		await _sut.InvalidateNodesOnParameterMismatchAsync(
			dataFlowExecutionContext, upstreamSubgraph, providedDynamicParameters, CancellationToken.None);

		// Assert
		await _dataFlowInvalidator.Received(1)
			.InvalidateNodeAndDownstreamAsync(dataFlowExecutionContext, nodeId, CancellationToken.None);
	}

	[Fact]
	public async Task InvalidateNodesOnParameterMismatchAsync_ShouldInvalidateAllConflictedNodes_WhenMultipleNodesHaveDifferentParameterValues()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId1 = new NodeId(Guid.NewGuid());
		var nodeId2 = new NodeId(Guid.NewGuid());
		var dataFlowExecutionContext = CreateContext(executionId);

		IReadOnlyGraph upstreamSubgraph = Substitute.For<IReadOnlyGraph>();
		upstreamSubgraph.GetAllNodes().Returns(new List<Node>
		{
			new() { Id = nodeId1 },
			new() { Id = nodeId2 }
		});

		List<DynamicParameter> providedDynamicParameters = [new() { Name = "age", Value = 30 }];

		IReadOnlyCollection<DynamicParameter> storedParams = [new DynamicParameter { Name = "age", Value = 10 }];
		_dataFlowDynamicParametersStore
			.TryGetDynamicParameters(executionId, Arg.Any<NodeId>(), out Arg.Any<IReadOnlyCollection<DynamicParameter>>())
			.Returns(callInfo =>
			{
				callInfo[2] = storedParams;
				return true;
			});

		// Act
		await _sut.InvalidateNodesOnParameterMismatchAsync(
			dataFlowExecutionContext, upstreamSubgraph, providedDynamicParameters, CancellationToken.None);

		// Assert
		await _dataFlowInvalidator.Received(1)
			.InvalidateNodeAndDownstreamAsync(dataFlowExecutionContext, nodeId1, CancellationToken.None);
		await _dataFlowInvalidator.Received(1)
			.InvalidateNodeAndDownstreamAsync(dataFlowExecutionContext, nodeId2, CancellationToken.None);
	}

	[Fact]
	public async Task InvalidateNodesOnParameterMismatchAsync_ShouldInvalidateNode_WhenOneProvidedParameterMatchesStoredNameWithDifferentValue()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		var dataFlowExecutionContext = CreateContext(executionId);

		IReadOnlyGraph upstreamSubgraph = Substitute.For<IReadOnlyGraph>();
		upstreamSubgraph.GetAllNodes().Returns(new List<Node> { new() { Id = nodeId } });

		List<DynamicParameter> providedDynamicParameters =
		[
			new() { Name = "age", Value = 30 },
			new() { Name = "country", Value = "NL" }
		];

		IReadOnlyCollection<DynamicParameter> storedParams = [new DynamicParameter { Name = "age", Value = 10 }];
		_dataFlowDynamicParametersStore
			.TryGetDynamicParameters(executionId, nodeId, out Arg.Any<IReadOnlyCollection<DynamicParameter>>())
			.Returns(callInfo =>
			{
				callInfo[2] = storedParams;
				return true;
			});

		// Act
		await _sut.InvalidateNodesOnParameterMismatchAsync(
			dataFlowExecutionContext, upstreamSubgraph, providedDynamicParameters, CancellationToken.None);

		// Assert
		await _dataFlowInvalidator.Received(1)
			.InvalidateNodeAndDownstreamAsync(dataFlowExecutionContext, nodeId, CancellationToken.None);
	}

	[Fact]
	public async Task InvalidateNodesOnParameterMismatchAsync_ShouldNotInvalidate_WhenNoStoredParameterNameMatchesProvided()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		var dataFlowExecutionContext = CreateContext(executionId);

		IReadOnlyGraph upstreamSubgraph = Substitute.For<IReadOnlyGraph>();
		upstreamSubgraph.GetAllNodes().Returns(new List<Node> { new() { Id = nodeId } });

		List<DynamicParameter> providedDynamicParameters = [new() { Name = "age", Value = 30 }];

		IReadOnlyCollection<DynamicParameter> storedParams = [new DynamicParameter { Name = "country", Value = "NL" }];
		_dataFlowDynamicParametersStore
			.TryGetDynamicParameters(executionId, nodeId, out Arg.Any<IReadOnlyCollection<DynamicParameter>>())
			.Returns(callInfo =>
			{
				callInfo[2] = storedParams;
				return true;
			});

		// Act
		await _sut.InvalidateNodesOnParameterMismatchAsync(
			dataFlowExecutionContext, upstreamSubgraph, providedDynamicParameters, CancellationToken.None);

		// Assert
		await _dataFlowInvalidator.DidNotReceive()
			.InvalidateNodeAndDownstreamAsync(Arg.Any<DataFlowExecutionContext>(), Arg.Any<NodeId>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task InvalidateNodesOnParameterMismatchAsync_ShouldNotInvalidate_WhenNodeHasNoStoredDynamicParameters()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		var dataFlowExecutionContext = CreateContext(executionId);

		IReadOnlyGraph upstreamSubgraph = Substitute.For<IReadOnlyGraph>();
		upstreamSubgraph.GetAllNodes().Returns(new List<Node> { new() { Id = nodeId } });

		List<DynamicParameter> providedDynamicParameters = [new() { Name = "age", Value = 30 }];

		_dataFlowDynamicParametersStore
			.TryGetDynamicParameters(executionId, nodeId, out Arg.Any<IReadOnlyCollection<DynamicParameter>>())
			.Returns(false);

		// Act
		await _sut.InvalidateNodesOnParameterMismatchAsync(
			dataFlowExecutionContext, upstreamSubgraph, providedDynamicParameters, CancellationToken.None);

		// Assert
		await _dataFlowInvalidator.DidNotReceive()
			.InvalidateNodeAndDownstreamAsync(Arg.Any<DataFlowExecutionContext>(), Arg.Any<NodeId>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task InvalidateNodesOnParameterMismatchAsync_ShouldNotInvalidate_WhenProvidedParametersAreEmpty()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		var dataFlowExecutionContext = CreateContext(executionId);

		IReadOnlyGraph upstreamSubgraph = Substitute.For<IReadOnlyGraph>();
		upstreamSubgraph.GetAllNodes().Returns(new List<Node> { new() { Id = nodeId } });

		// Act
		await _sut.InvalidateNodesOnParameterMismatchAsync(
			dataFlowExecutionContext, upstreamSubgraph, [], CancellationToken.None);

		// Assert
		await _dataFlowInvalidator.DidNotReceive()
			.InvalidateNodeAndDownstreamAsync(Arg.Any<DataFlowExecutionContext>(), Arg.Any<NodeId>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task InvalidateNodesOnParameterMismatchAsync_ShouldNotInvalidate_WhenParameterNamesDifferOnlyByCase()
	{
		// Arrange — names are matched with ordinal equality; "age" is not the same parameter as "Age".
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		var dataFlowExecutionContext = CreateContext(executionId);

		IReadOnlyGraph upstreamSubgraph = Substitute.For<IReadOnlyGraph>();
		upstreamSubgraph.GetAllNodes().Returns(new List<Node> { new() { Id = nodeId } });

		List<DynamicParameter> providedDynamicParameters = [new() { Name = "age", Value = 30 }];

		IReadOnlyCollection<DynamicParameter> storedParams = [new DynamicParameter { Name = "Age", Value = 10 }];
		_dataFlowDynamicParametersStore
			.TryGetDynamicParameters(executionId, nodeId, out Arg.Any<IReadOnlyCollection<DynamicParameter>>())
			.Returns(callInfo =>
			{
				callInfo[2] = storedParams;
				return true;
			});

		// Act
		await _sut.InvalidateNodesOnParameterMismatchAsync(
			dataFlowExecutionContext, upstreamSubgraph, providedDynamicParameters, CancellationToken.None);

		// Assert
		await _dataFlowInvalidator.DidNotReceive()
			.InvalidateNodeAndDownstreamAsync(Arg.Any<DataFlowExecutionContext>(), Arg.Any<NodeId>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task InvalidateNodesOnParameterMismatchAsync_ShouldNotInvalidate_WhenMatchingStoredParameterHasSameValueAsProvided()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		DataFlowExecutionContext dataFlowExecutionContext = CreateContext(executionId);

		IReadOnlyGraph upstreamSubgraph = Substitute.For<IReadOnlyGraph>();
		upstreamSubgraph.GetAllNodes().Returns(new List<Node> { new() { Id = nodeId } });

		List<DynamicParameter> providedDynamicParameters = [new() { Name = "age", Value = 30 }];

		IReadOnlyCollection<DynamicParameter> storedParams = [new DynamicParameter { Name = "age", Value = 30 }];
		_dataFlowDynamicParametersStore
			.TryGetDynamicParameters(executionId, nodeId, out Arg.Any<IReadOnlyCollection<DynamicParameter>>())
			.Returns(callInfo =>
			{
				callInfo[2] = storedParams;
				return true;
			});

		// Act
		await _sut.InvalidateNodesOnParameterMismatchAsync(
			dataFlowExecutionContext, upstreamSubgraph, providedDynamicParameters, CancellationToken.None);

		// Assert
		await _dataFlowInvalidator.DidNotReceive()
			.InvalidateNodeAndDownstreamAsync(Arg.Any<DataFlowExecutionContext>(), Arg.Any<NodeId>(), Arg.Any<CancellationToken>());
	}

	private static DataFlowExecutionContext CreateContext(DataFlowExecutionId executionId) =>
		new()
		{
			DataFlowExecutionId = executionId,
			DataFlowVersion = new DataFlowVersion(2),
			Graph = Substitute.For<IGraph>(),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
}
