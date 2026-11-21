using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using DataFlowRunner.Common.ApiConfiguration;
using DataFlowRunner.DataFlowAuthorization.Attributes;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.ProvideRequiredDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataFlowRunner.DataFlowDynamicParameters.Controllers;

[ApiController]
[Route($"{ApiRoutes.Base}/{ApiRoutes.DataFlowExecutions}/{{dataFlowExecutionId:guid}}")]
[ApiVersion("1")]
[AuthorizeDataFlowExecution]
public sealed class DynamicParameterController : ControllerBase
{
	private readonly IRequiredDynamicParametersProvider _requiredDynamicParametersProvider;

	public DynamicParameterController(IRequiredDynamicParametersProvider requiredDynamicParametersProvider)
	{
		_requiredDynamicParametersProvider = requiredDynamicParametersProvider ??
		                            throw new ArgumentNullException(nameof(requiredDynamicParametersProvider));
	}

	[HttpGet("data-flow-dynamic-parameters")]
	public Task<List<DynamicParameterDto>> GetDataFlowDynamicParameters(Guid dataFlowExecutionId,
		[Required] long dataFlowVersion,
		CancellationToken cancellationToken)
	{
		return _requiredDynamicParametersProvider.ProvideDataFlowDynamicParametersAsync(
			dataFlowExecutionId,
			dataFlowVersion,
			cancellationToken);
	}

	[HttpGet("data-flow-dynamic-parameters/{nodeId:guid}")]
	public Task<List<DynamicParameterDto>> GetDataFlowNodeExecutionDynamicParameters(Guid dataFlowExecutionId,
		Guid nodeId,
		[Required] long dataFlowVersion,
		CancellationToken cancellationToken)
	{
		return _requiredDynamicParametersProvider.ProvideDataFlowDynamicParametersForRunningNodeAsync(
			dataFlowExecutionId,
			nodeId,
			dataFlowVersion,
			cancellationToken);
	}
}
