using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Infrastructure;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.RunDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Business.RunDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Exceptions;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.Tests.DataFlowDynamicParameters.Business.RunDynamicParameters;

public sealed class RunDynamicParametersByNodeResolverTests
{
	private readonly IDynamicParametersFetcher _dynamicParametersFetcher;
	private readonly IRunDynamicParametersValidator _runDynamicParametersValidator;
	private readonly RunDynamicParametersByNodeResolver _sut;

	public RunDynamicParametersByNodeResolverTests()
	{
		_dynamicParametersFetcher = Substitute.For<IDynamicParametersFetcher>();
		_runDynamicParametersValidator = Substitute.For<IRunDynamicParametersValidator>();
		_sut = new RunDynamicParametersByNodeResolver(_dynamicParametersFetcher, _runDynamicParametersValidator);
	}

	[Fact]
	public async Task ResolveProvidedParametersByNodeAsync_ShouldValidateAndReturnMappedParametersPerNode_WhenProvidedParametersSatisfyAllRequiredNames()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var node1Id = new NodeId(Guid.NewGuid());
		var node2Id = new NodeId(Guid.NewGuid());
		var nodes = new List<Node>
		{
			new() { Id = node1Id },
			new() { Id = node2Id }
		};
		var provided = new List<DynamicParameter>
		{
			new() { Name = "param1", Value = "a" },
			new() { Name = "param2", Value = 42 },
			new() { Name = "param3", Value = "unused" }
		};
		var fetchResponse = new ResponseDataFlowDynamicParametersDto(
		[
			new ResponseNodeDynamicParametersDto(node1Id.Value,
			[
				new ResponseDataFlowDynamicParameterDto("id-1", "param1", true, "TEXT", false, null)
			]),
			new ResponseNodeDynamicParametersDto(node2Id.Value,
			[
				new ResponseDataFlowDynamicParameterDto("id-2", "param2", true, "INTEGER", false, null)
			])
		]);
		var expected = new Dictionary<NodeId, List<DynamicParameter>>
		{
			[node1Id] = [new() { Name = "param1", Value = "a" }],
			[node2Id] = [new() { Name = "param2", Value = 42 }]
		};
		_dynamicParametersFetcher
			.FetchDataFlowDynamicParametersAsync(
				executionId,
				Arg.Is<List<NodeId>>(ids => ids.SequenceEqual(new[] { node1Id, node2Id })),
				CancellationToken.None)
			.Returns(fetchResponse);

		// Act
		IReadOnlyDictionary<NodeId, List<DynamicParameter>> result = await _sut.ResolveProvidedParametersByNodeAsync(
			executionId, nodes, provided, CancellationToken.None);

		// Assert
		result.Should().HaveCount(2).And.BeEquivalentTo(expected);
		await _dynamicParametersFetcher.Received(1).FetchDataFlowDynamicParametersAsync(
			executionId,
			Arg.Is<List<NodeId>>(ids => ids.SequenceEqual(new[] { node1Id, node2Id })),
			CancellationToken.None);
		_runDynamicParametersValidator.Received(1).EnsureProvidedParametersAreValid(fetchResponse, provided);
	}

	[Fact]
	public async Task ResolveProvidedParametersByNodeAsync_ShouldReturnMappedAndEmptyParameters_WhenOnlySomeNodesHaveFetchEntry()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeWithFetch = new NodeId(Guid.NewGuid());
		var nodeWithoutFetch = new NodeId(Guid.NewGuid());
		var nodes = new List<Node>
		{
			new() { Id = nodeWithFetch },
			new() { Id = nodeWithoutFetch }
		};
		var provided = new List<DynamicParameter> { new() { Name = "param1", Value = "a" } };
		var fetchResponse = new ResponseDataFlowDynamicParametersDto(
		[
			new ResponseNodeDynamicParametersDto(nodeWithFetch.Value,
			[
				new ResponseDataFlowDynamicParameterDto("id-1", "param1", true, "TEXT", false, null)
			])
		]);
		var expected = new Dictionary<NodeId, List<DynamicParameter>>
		{
			[nodeWithFetch] = [new() { Name = "param1", Value = "a" }],
			[nodeWithoutFetch] = []
		};
		_dynamicParametersFetcher
			.FetchDataFlowDynamicParametersAsync(
				executionId,
				Arg.Is<List<NodeId>>(ids => ids.SequenceEqual(new[] { nodeWithFetch, nodeWithoutFetch })),
				CancellationToken.None)
			.Returns(fetchResponse);

		// Act
		IReadOnlyDictionary<NodeId, List<DynamicParameter>> result = await _sut.ResolveProvidedParametersByNodeAsync(
			executionId, nodes, provided, CancellationToken.None);

		// Assert
		result.Should().HaveCount(2).And.BeEquivalentTo(expected);
		await _dynamicParametersFetcher.Received(1).FetchDataFlowDynamicParametersAsync(
			executionId,
			Arg.Is<List<NodeId>>(ids => ids.SequenceEqual(new[] { nodeWithFetch, nodeWithoutFetch })),
			CancellationToken.None);
		_runDynamicParametersValidator.Received(1).EnsureProvidedParametersAreValid(fetchResponse, provided);
	}

	[Fact]
	public async Task ResolveProvidedParametersByNodeAsync_ShouldThrowDynamicParameterNotFilledException_WhenValidatorThrows()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId = new NodeId(Guid.NewGuid());
		var nodes = new List<Node> { new() { Id = nodeId } };
		var provided = new List<DynamicParameter> { new() { Name = "param1", Value = "a" } };
		var fetchResponse = new ResponseDataFlowDynamicParametersDto(
		[
			new ResponseNodeDynamicParametersDto(nodeId.Value,
			[
				new ResponseDataFlowDynamicParameterDto("id-1", "param1", true, "TEXT", false, null)
			])
		]);
		_dynamicParametersFetcher
			.FetchDataFlowDynamicParametersAsync(
				executionId,
				Arg.Is<List<NodeId>>(ids => ids.SequenceEqual(new[] { nodeId })),
				CancellationToken.None)
			.Returns(fetchResponse);
		_runDynamicParametersValidator
			.When(validator => validator.EnsureProvidedParametersAreValid(fetchResponse, provided))
			.Throw(new DynamicParameterNotFilledException("invalid"));

		// Act
		Func<Task> act = async () =>
			await _sut.ResolveProvidedParametersByNodeAsync(executionId, nodes, provided, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<DynamicParameterNotFilledException>()
			.WithMessage("invalid");
		await _dynamicParametersFetcher.Received(1).FetchDataFlowDynamicParametersAsync(
			executionId,
			Arg.Is<List<NodeId>>(ids => ids.SequenceEqual(new[] { nodeId })),
			CancellationToken.None);
	}
}
