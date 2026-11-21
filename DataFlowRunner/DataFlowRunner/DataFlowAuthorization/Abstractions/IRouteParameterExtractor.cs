using DataFlowRunner.DataFlowAuthorization.Exceptions;
using DataFlowRunner.DataFlowExecution.Models;

namespace DataFlowRunner.DataFlowAuthorization.Abstractions;

/// <summary>
/// Extracts route parameters from HTTP context.
/// </summary>
internal interface IRouteParameterExtractor
{
	/// <summary>
	/// Extracts the data flow execution ID from the route parameters.
	/// </summary>
	/// <param name="context">The HTTP context containing route values.</param>
	/// <returns>The data flow execution ID in the route.</returns>
	/// <exception cref="DataFlowExecutionIdNotFoundInRouteException">Thrown when data flow execution id not found in route.</exception>
	DataFlowExecutionId ExtractDataFlowExecutionId(HttpContext context);
}
