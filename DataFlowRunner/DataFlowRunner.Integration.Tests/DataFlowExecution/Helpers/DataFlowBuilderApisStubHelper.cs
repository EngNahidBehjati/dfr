using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowSnapshot.Models;
using DataFlowRunner.Integration.Tests.Common.Wiremock;
using Microsoft.AspNetCore.Http;

namespace DataFlowRunner.Integration.Tests.DataFlowExecution.Helpers;

internal sealed class DataFlowBuilderApisStubHelper
{
	private readonly DataFlowBuilderServerFixture _dataFlowBuilderServerFixture;

	public DataFlowBuilderApisStubHelper(DataFlowBuilderServerFixture dataFlowBuilderServerFixture)
	{
		_dataFlowBuilderServerFixture = dataFlowBuilderServerFixture ??
		                                throw new ArgumentNullException(nameof(dataFlowBuilderServerFixture));
	}

	public void StubTopologyResponse(Guid executionId, TopologyDto topology, long dataFlowVersion)
	{
		var dataFlowTopology = new DataFlowTopologyDto(topology, DataFlowVersion: dataFlowVersion);
		_dataFlowBuilderServerFixture.StubJsonResponse(
			$"/api/v2/pipelines/{executionId}/topology",
			HttpMethod.Get,
			dataFlowTopology);
	}

	public void StubTopologyFailResponse(Guid executionId, string message, string stackTrace)
	{
		var response = new Dictionary<string, string>()
		{
			["message"] = message,
			["stackTrace"] = stackTrace
		};
		_dataFlowBuilderServerFixture.StubJsonResponse(
			$"/api/v2/pipelines/{executionId}/topology",
			HttpMethod.Get,
			response,
			StatusCodes.Status500InternalServerError);
	}

	public void StubDynamicParametersResponse(Guid executionId)
	{
		StubDynamicParametersResponse(executionId, new Dictionary<Guid, IReadOnlyCollection<ResponseDataFlowDynamicParameterDto>>());
	}

	public void StubDynamicParametersResponse(Guid executionId,
		IReadOnlyDictionary<Guid, IReadOnlyCollection<ResponseDataFlowDynamicParameterDto>> dynamicParametersByNode)
	{
		IReadOnlyCollection<ResponseNodeDynamicParametersDto> nodeDynamicParametersDtos = dynamicParametersByNode
			.Select(parametersByNode =>
				new ResponseNodeDynamicParametersDto(parametersByNode.Key, parametersByNode.Value))
			.ToList();

		var dataFlowDynamicParameters = new ResponseDataFlowDynamicParametersDto(nodeDynamicParametersDtos);
		_dataFlowBuilderServerFixture.StubJsonResponse(
			$"/api/v1/pipelines/{executionId}/simple-variables",
			HttpMethod.Post,
			dataFlowDynamicParameters);
	}

	public void StubNodeConfigsResponse(Guid executionId, TopologyDto topology)
	{
		var configs = topology.Nodes.Select(node => new Dictionary<string, object>
		{
			["pluginId"] = node.NodeId,
			["version"] = node.Version.ToString(),
			["pluginExeJson"] = new Dictionary<string, object>()
		}).ToList();

		var response = new Dictionary<string, object>
		{
			["exeConfigList"] = configs
		};

		_dataFlowBuilderServerFixture.StubJsonResponse(
			$"/api/v1/state/{executionId}/config",
			HttpMethod.Post,
			response);
	}

	public void StubNodeConfigsFailResponse(Guid executionId, string message, string stackTrace)
	{
		var response = new Dictionary<string, string>()
		{
			["message"] = message,
			["stackTrace"] = stackTrace
		};
		_dataFlowBuilderServerFixture.StubJsonResponse(
			$"/api/v1/state/{executionId}/config",
			HttpMethod.Post,
			response,
			StatusCodes.Status500InternalServerError);
	}
}
