using System.Net;
using DataFlowRunner.Integration.Tests.DataFlowExecution.Helpers;
using DataFlowRunner.Integration.Tests.DataFlowExecution.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DataFlowRunner.Integration.Tests.DataFlowChangeEvents.Helpers;

internal sealed class CancelDataFlowHelper
{
	private readonly DataFlowExecutionRequestHelper _requestHelper;
	private readonly DataFlowExecutionAssertionsHelper _assertionsHelper;

	public CancelDataFlowHelper(WebApplicationFactory<Program> factory)
	{
		_requestHelper = new DataFlowExecutionRequestHelper(factory);
		_assertionsHelper = new DataFlowExecutionAssertionsHelper(factory);
	}

	public async Task Cancel(Guid executionId)
	{
		var cancelDataFlowUrl = BuildUrlHelper.BuildCancelDataFlowUrl(executionId);
		DataFlowExecutionResult result = await _requestHelper.ExecuteRequestAsync(cancelDataFlowUrl);
		_assertionsHelper.AssertStatusCode(result, HttpStatusCode.OK);
	}
}
