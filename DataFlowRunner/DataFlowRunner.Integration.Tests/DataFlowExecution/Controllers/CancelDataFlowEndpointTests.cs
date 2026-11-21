using System.Net;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Commands;
using DataFlowRunner.DataFlowSnapshot.Models;
using DataFlowRunner.Integration.Tests.Common;
using DataFlowRunner.Integration.Tests.Common.Wiremock;
using DataFlowRunner.Integration.Tests.DataFlowExecution.Helpers;
using DataFlowRunner.Integration.Tests.DataFlowExecution.Models;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DataFlowRunner.Integration.Tests.DataFlowExecution.Controllers;

public sealed class CancelDataFlowEndpointTests : IClassFixture<DataFlowBuilderServerFixture>,
	IClassFixture<AmServerFixture>,
	IDisposable
{
	private readonly WebApplicationFactory<Program> _factory;
	private readonly DataFlowBuilderApisStubHelper _dataFlowBuilderApisStubHelper;
	private readonly DataFlowExecutionRequestHelper _requestHelper;
	private readonly DataFlowExecutionAssertionsHelper _assertionsHelper;
	private readonly WolverineStubConfiguration _wolverineStubConfiguration;

	public CancelDataFlowEndpointTests(DataFlowBuilderServerFixture dataFlowBuilderServerFixture,
		AmServerFixture amServerFixture)
	{
		ArgumentNullException.ThrowIfNull(dataFlowBuilderServerFixture);
		ArgumentNullException.ThrowIfNull(amServerFixture);

		_factory = WebApplicationFactoryCreator.Create(
			dataFlowBuilderServerFixture.BaseAddress,
			amServerFixture.BaseAddress);

		_dataFlowBuilderApisStubHelper = new DataFlowBuilderApisStubHelper(dataFlowBuilderServerFixture);
		_requestHelper = new DataFlowExecutionRequestHelper(_factory);
		_assertionsHelper = new DataFlowExecutionAssertionsHelper(_factory);
		_wolverineStubConfiguration = new WolverineStubConfiguration(_factory);
	}

	[Fact]
	public async Task CancelDataFlow_ShouldChangeStateOfAllNodes_WhenCancelIsCalled()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 1;
		var topologyDtoCreator = new TopologyDtoCreator();
		TopologyDto topology = topologyDtoCreator.Create();
		_dataFlowBuilderApisStubHelper.StubTopologyResponse(executionId, topology, dataFlowVersion);
		_dataFlowBuilderApisStubHelper.StubNodeConfigsResponse(executionId, topology);
		_dataFlowBuilderApisStubHelper.StubDynamicParametersResponse(executionId);

		var nodeErrorMessage = new NodeErrorMessage { Message = "Error in running", StackTrace = null };
		var wolverineStubOptions = new WolverineStubOptions
		{
			NodeFailureMessages = new Dictionary<Guid, NodeErrorMessage>
			{
				{ topologyDtoCreator.NodeBNodeId, nodeErrorMessage }
			},
			NodesWithoutExecutionReport = new HashSet<Guid> { topologyDtoCreator.NodeANodeId }
		};
		_wolverineStubConfiguration.ConfigureWolverineStubs(wolverineStubOptions);

		var runDataFlowUrl = BuildUrlHelper.BuildRunDataFlowUrl(executionId, dataFlowVersion);
		var cancelDataFlowUrl = BuildUrlHelper.BuildCancelDataFlowUrl(executionId);
		var expectedNodeStates = new Dictionary<Guid, NodeExecutionState>
		{
			{ topologyDtoCreator.RootNodeId, NodeExecutionState.Succeeded },
			{ topologyDtoCreator.NodeANodeId, NodeExecutionState.Canceled },
			{ topologyDtoCreator.NodeAChildNodeId, NodeExecutionState.Idle },
			{ topologyDtoCreator.NodeBNodeId, NodeExecutionState.Failed },
			{ topologyDtoCreator.JoinNodeId, NodeExecutionState.Canceled }
		};

		// Act
		_ = await _requestHelper.ExecuteRequestAsync(runDataFlowUrl);
		DataFlowExecutionResult cancelResult = await _requestHelper.ExecuteRequestAsync(cancelDataFlowUrl);

		// Assert
		_assertionsHelper.AssertStatusCode(cancelResult, HttpStatusCode.OK);
		_assertionsHelper.AssertNodeStates(executionId, expectedNodeStates);
		var publishedCancelNodeCommandNodeIds = cancelResult
			.GetPublishedCommands<CancelNodeWolverineCommand>(executionId)
			.Select(command => command.NodeId)
			.ToList();

		publishedCancelNodeCommandNodeIds.Should().HaveCount(1);
		publishedCancelNodeCommandNodeIds[0].Should().Be(topologyDtoCreator.NodeANodeId);
	}


	public void Dispose()
	{
		_factory.Dispose();
	}
}
