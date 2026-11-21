using System.Net;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.Integration.Tests.DataFlowExecution.Models;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DataFlowRunner.Integration.Tests.DataFlowExecution.Helpers;

internal sealed class DataFlowExecutionAssertionsHelper
{
	private readonly INodeExecutionStateStore _stateStore;
	private readonly INodeErrorMessageStore _errorMessageStore;

	public DataFlowExecutionAssertionsHelper(WebApplicationFactory<Program> factory)
	{
		_stateStore = factory.Services.GetRequiredService<INodeExecutionStateStore>();
		_errorMessageStore = factory.Services.GetRequiredService<INodeErrorMessageStore>();
	}

	public void AssertStatusCode(DataFlowExecutionResult result, HttpStatusCode expectedStatusCode)
	{
		result.HttpResponseContainer.Response.StatusCode.Should()
			.Be(expectedStatusCode, $"response body: {result.HttpResponseContainer.ResponseBody}");
	}

	public void AssertNodeStates(Guid executionId, IReadOnlyDictionary<Guid, NodeExecutionState> expectedNodeStates)
	{
		var dataFlowExecutionId = new DataFlowExecutionId(executionId);
		var actualStates =
			_stateStore.GetAllNodeExecutionStates(dataFlowExecutionId)
			.ToDictionary(pair => pair.Key.Value, pair => pair.Value);

		actualStates.Should().BeEquivalentTo(expectedNodeStates);
	}

	public void AssertErrorMessage(Guid executionId, IReadOnlyDictionary<Guid, NodeErrorMessage> expectedErrorMessage)
	{
		foreach ((Guid nodeId, NodeErrorMessage errorMessage) in expectedErrorMessage)
		{
			NodeErrorMessage actualErrorMessage = _errorMessageStore.GetNodeErrorMessage(executionId, nodeId);
			actualErrorMessage.Should().BeEquivalentTo(errorMessage);
		}
	}
}
