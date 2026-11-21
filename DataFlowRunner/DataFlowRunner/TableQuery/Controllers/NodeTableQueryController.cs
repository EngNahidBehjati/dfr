using Asp.Versioning;
using DataFlowRunner.Common.ApiConfiguration;
using DataFlowRunner.DataFlowAuthorization.Attributes;
using DataFlowRunner.TableQuery.Abstractions;
using DataFlowRunner.TableQuery.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataFlowRunner.TableQuery.Controllers;

[ApiController]
[Route($"{ApiRoutes.Base}/{ApiRoutes.DataFlowExecutions}/{{dataFlowExecutionId:guid}}/nodes/{{nodeId:guid}}/ports/{{outputPortId:guid}}/table")]
[ApiVersion("1")]
[AuthorizeDataFlowExecution]
public sealed class NodeTableQueryController : ControllerBase
{
	private readonly ITableQueryFacade _tableQueryFacade;
	private readonly IQueryParametersExtractor _queryParametersExtractor;

	public NodeTableQueryController(ITableQueryFacade tableQueryFacade, IQueryParametersExtractor queryParametersExtractor)
	{
		_tableQueryFacade = tableQueryFacade ?? throw new ArgumentNullException(nameof(tableQueryFacade));
		_queryParametersExtractor = queryParametersExtractor ?? throw new ArgumentNullException(nameof(queryParametersExtractor));
	}

	/// <summary>
	/// Retrieves table data for a specific data flow execution node.
	/// Returns the table schema and rows based on the specified parameters.
	/// </summary>
	/// <param name="dataFlowExecutionId">The unique identifier of the data flow execution.</param>
	/// <param name="nodeId">The unique identifier of the node within the data flow.</param>
	/// <param name="outputPortId">output port id of node.</param>
	/// <param name="includeSchema">Whether to include the table schema (column definitions) in the response.</param>
	/// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
	/// <returns>Table response with schema and rows.</returns>
	[HttpGet]
	public async Task<TableQueryResponseDto> Query(
		Guid dataFlowExecutionId,
		Guid nodeId,
		Guid outputPortId,
		bool includeSchema,
		CancellationToken cancellationToken = default)
	{
		IReadOnlyDictionary<string, string> queryParams =
			_queryParametersExtractor.GetParameters(Request.Query);

		return await _tableQueryFacade.QueryTableAsync(
			dataFlowExecutionId,
			nodeId,
			outputPortId,
			includeSchema,
			queryParams,
			cancellationToken)
			.ConfigureAwait(false);
	}

	/// <summary>
	/// Retrieves the total number of rows in the table for a specific data flow execution node.
	/// </summary>
	/// <param name="dataFlowExecutionId">The unique identifier of the data flow execution.</param>
	/// <param name="nodeId">The unique identifier of the node within the data flow.</param>
	/// <param name="outputPortId">output port id of node.</param>
	/// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
	/// <returns>Total number of rows in the table.</returns>
	[HttpGet("row-count")]
	public async Task<long> Count(
		Guid dataFlowExecutionId,
		Guid nodeId,
		Guid outputPortId,
		CancellationToken cancellationToken = default)
	{
		IReadOnlyDictionary<string, string> queryParams =
			_queryParametersExtractor.GetParameters(Request.Query);

		return await _tableQueryFacade.GetRowCountAsync(
			dataFlowExecutionId,
			nodeId,
			outputPortId,
			queryParams,
			cancellationToken)
			.ConfigureAwait(false);
	}
}
