using DataFlowRunner.Common.Authentication.Abstractions;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Infrastructure;
using DataFlowRunner.DataFlowDynamicParameters.Business.Infrastructure;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowDynamicParameters.Business.Infrastructure;

public sealed class DynamicParametersFetcherTests
{
	private readonly IAuthenticationTokenAccessor _authenticationTokenAccessor;
	private readonly IDataFlowDynamicParametersClientApi _dataFlowDynamicParametersClientApi;
	private readonly DynamicParametersFetcher _sut;

	public DynamicParametersFetcherTests()
	{
		_authenticationTokenAccessor = Substitute.For<IAuthenticationTokenAccessor>();
		_dataFlowDynamicParametersClientApi = Substitute.For<IDataFlowDynamicParametersClientApi>();
		_sut = new DynamicParametersFetcher(_authenticationTokenAccessor, _dataFlowDynamicParametersClientApi);
	}

	[Fact]
	public async Task FetchDataFlowDynamicParametersAsync_ShouldCallApiWithAuthenticationCookieAndEmptyNodeList_WhenNoNodesAreRequested()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		CancellationToken cancellationToken = CancellationToken.None;
		var authenticationCookie = new AuthenticationCookie("test-cookie");
		var expected = new ResponseDataFlowDynamicParametersDto([]);
		_authenticationTokenAccessor.GetAuthenticationCookie().Returns(authenticationCookie);
		_dataFlowDynamicParametersClientApi
			.GetDataFlowDynamicParametersAsync(
				executionId.Value,
				authenticationCookie,
				Arg.Is<RequestDataFlowDynamicParametersDto>(r => r.NodeIds.Count == 0),
				cancellationToken)
			.Returns(expected);

		// Act
		ResponseDataFlowDynamicParametersDto actual =
			await _sut.FetchDataFlowDynamicParametersAsync(executionId, [], cancellationToken);

		// Assert
		actual.Should().BeSameAs(expected);
		await _dataFlowDynamicParametersClientApi.Received(1).GetDataFlowDynamicParametersAsync(
			executionId.Value,
			authenticationCookie,
			Arg.Is<RequestDataFlowDynamicParametersDto>(r => r.NodeIds.Count == 0),
			cancellationToken);
	}

	[Fact]
	public async Task FetchDataFlowDynamicParametersAsync_ShouldMapNodeIdsToRequestDto_WhenNodesAreRequested()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var nodeId1 = Guid.NewGuid();
		var nodeId2 = Guid.NewGuid();
		var nodeIds = new List<NodeId> { new(nodeId1), new(nodeId2) };
		CancellationToken cancellationToken = CancellationToken.None;
		var authenticationCookie = new AuthenticationCookie("test-cookie");
		var expected = new ResponseDataFlowDynamicParametersDto([]);
		_authenticationTokenAccessor.GetAuthenticationCookie().Returns(authenticationCookie);
		_dataFlowDynamicParametersClientApi
			.GetDataFlowDynamicParametersAsync(
				executionId.Value,
				authenticationCookie,
				Arg.Is<RequestDataFlowDynamicParametersDto>(r =>
					r.NodeIds.SequenceEqual(new[] { nodeId1, nodeId2 })),
				cancellationToken)
			.Returns(expected);

		// Act
		ResponseDataFlowDynamicParametersDto actual =
			await _sut.FetchDataFlowDynamicParametersAsync(executionId, nodeIds, cancellationToken);

		// Assert
		actual.Should().BeSameAs(expected);
		await _dataFlowDynamicParametersClientApi.Received(1).GetDataFlowDynamicParametersAsync(
			executionId.Value,
			authenticationCookie,
			Arg.Is<RequestDataFlowDynamicParametersDto>(r =>
				r.NodeIds.SequenceEqual(new[] { nodeId1, nodeId2 })),
			cancellationToken);
	}
}
