using Asp.Versioning;
using DataFlowRunner.Common.ApiConfiguration;
using DataFlowRunner.DataFlowAuthorization.Attributes;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;

namespace DataFlowRunner.NodeExecutionStates.Controllers;

[ApiController]
[Route(
	$"{ApiRoutes.Base}/{ApiRoutes.DataFlowExecutions}/{{dataFlowExecutionId:guid}}")]
[ApiVersion("1")]
[AuthorizeDataFlowExecution]
public sealed class DataFlowExecutionStateController : ControllerBase
{
	private readonly INodeExecutionStatesDtoProvider _nodeExecutionStatesDtoProvider;
	private readonly IDataFlowExecutionStatusDtoProvider _dataFlowExecutionStatusDtoProvider;

	public DataFlowExecutionStateController(INodeExecutionStatesDtoProvider nodeExecutionStatesDtoProvider,
		IDataFlowExecutionStatusDtoProvider dataFlowExecutionStatusDtoProvider)
	{
		_nodeExecutionStatesDtoProvider =
			nodeExecutionStatesDtoProvider ?? throw new ArgumentNullException(nameof(nodeExecutionStatesDtoProvider));
		_dataFlowExecutionStatusDtoProvider = dataFlowExecutionStatusDtoProvider ?? throw new ArgumentNullException(nameof(dataFlowExecutionStatusDtoProvider));
	}

	[HttpGet("node-states")]
	public Task<DataFlowNodeExecutionStatesDto> GetNodeStates(Guid dataFlowExecutionId,
		CancellationToken cancellationToken)
	{
		return _nodeExecutionStatesDtoProvider.GetAllNodeExecutionStatesAsync(dataFlowExecutionId, cancellationToken);
	}

	[HttpGet("execution-state")]
	public Task<DataFlowExecutionStatusDto> GetExecutionState(Guid dataFlowExecutionId,
		CancellationToken cancellationToken)
	{
		return _dataFlowExecutionStatusDtoProvider
			.GetDataFlowExecutionStatusAsync(dataFlowExecutionId, cancellationToken);
	}
}
