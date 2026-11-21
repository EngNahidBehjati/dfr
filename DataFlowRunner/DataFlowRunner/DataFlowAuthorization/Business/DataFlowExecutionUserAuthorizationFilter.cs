using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DataFlowRunner.DataFlowAuthorization.Business;

internal sealed class DataFlowExecutionUserAuthorizationFilter : IAsyncAuthorizationFilter
{
	private readonly IRouteParameterExtractor _routeParameterExtractor;
	private readonly IDataFlowExecutionUserAuthorizer _dataFlowExecutionUserAuthorizer;

	public DataFlowExecutionUserAuthorizationFilter(
		IRouteParameterExtractor routeParameterExtractor,
		IDataFlowExecutionUserAuthorizer dataFlowExecutionUserAuthorizer)
	{
		_routeParameterExtractor = routeParameterExtractor ?? throw new ArgumentNullException(nameof(routeParameterExtractor));
		_dataFlowExecutionUserAuthorizer =
			dataFlowExecutionUserAuthorizer ?? throw new ArgumentNullException(nameof(dataFlowExecutionUserAuthorizer));
	}

	public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
	{
		ArgumentNullException.ThrowIfNull(context);

		DataFlowExecutionId dataFlowExecutionId = _routeParameterExtractor.ExtractDataFlowExecutionId(context.HttpContext);
		await _dataFlowExecutionUserAuthorizer.AuthorizeUserAccessAsync(
				dataFlowExecutionId,
				context.HttpContext.RequestAborted)
			.ConfigureAwait(false);
	}
}
