using System.Net;
using DataFlowRunner.DataFlowSnapshot.Models;
using DataFlowRunner.Integration.Tests.Common.Wiremock;
using DataFlowRunner.Integration.Tests.DataFlowExecution.Helpers;
using DataFlowRunner.Integration.Tests.DataFlowExecution.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DataFlowRunner.Integration.Tests.DataFlowChangeEvents.Helpers;

internal sealed class RunDataFlowHelper
{
	private readonly DataFlowBuilderApisStubHelper _dataFlowBuilderApisStubHelper;
	private readonly WolverineStubConfiguration _wolverineStubConfiguration;
	private readonly DataFlowExecutionRequestHelper _requestHelper;
	private readonly DataFlowExecutionAssertionsHelper _assertionsHelper;

	public RunDataFlowHelper(WebApplicationFactory<Program> factory,
		DataFlowBuilderServerFixture dataFlowBuilderServerFixture)
	{
		_dataFlowBuilderApisStubHelper = new DataFlowBuilderApisStubHelper(dataFlowBuilderServerFixture);
		_wolverineStubConfiguration = new WolverineStubConfiguration(factory);
		_requestHelper = new DataFlowExecutionRequestHelper(factory);
		_assertionsHelper = new DataFlowExecutionAssertionsHelper(factory);
	}

	public async Task Run(
		Guid executionId,
		long dataFlowVersion,
		TopologyDto topology,
		WolverineStubOptions wolverineStubOptions)
	{
		_dataFlowBuilderApisStubHelper.StubTopologyResponse(executionId, topology, dataFlowVersion);
		_dataFlowBuilderApisStubHelper.StubNodeConfigsResponse(executionId, topology);
		_dataFlowBuilderApisStubHelper.StubDynamicParametersResponse(executionId);
		_wolverineStubConfiguration.ConfigureWolverineStubs(wolverineStubOptions);

		var runDataFlowUrl = BuildUrlHelper.BuildRunDataFlowUrl(executionId, dataFlowVersion);

		DataFlowExecutionResult result = await _requestHelper.ExecuteRequestAsync(runDataFlowUrl);
		_assertionsHelper.AssertStatusCode(result, HttpStatusCode.OK);
	}

	public async Task Run(
		Guid executionId,
		long dataFlowVersion,
		TopologyDtoCreator topologyDtoCreator,
		WolverineStubOptions wolverineStubOptions)
	{
		await Run(executionId, dataFlowVersion, topologyDtoCreator.Create(), wolverineStubOptions);
	}

	public async Task Run(
		Guid executionId,
		long dataFlowVersion,
		TopologyDtoCreator topologyDtoCreator)
	{
		await Run(executionId, dataFlowVersion, topologyDtoCreator, new WolverineStubOptions());
	}
}
