using Asp.Versioning;
using DataFlowRunner.Common.ApiConfiguration;
using DataFlowRunner.DataFlowAuthorization.Attributes;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;

namespace DataFlowRunner.NodeExecutionStates.Controllers;

[ApiController]
[Route(
	$"{ApiRoutes.Base}/{ApiRoutes.DataFlowExecutions}/{{dataFlowExecutionId:guid}}/node-states/{{nodeId:guid}}")]
[ApiVersion("1")]
[AuthorizeDataFlowExecution]
public class NodeStateErrorMessageController : ControllerBase
{
	private readonly INodeErrorMessageDtoProvider _nodeErrorMessageDtoProvider;

	public NodeStateErrorMessageController(INodeErrorMessageDtoProvider nodeErrorMessageDtoProvider)
	{
		_nodeErrorMessageDtoProvider =
			nodeErrorMessageDtoProvider ?? throw new ArgumentNullException(nameof(nodeErrorMessageDtoProvider));
	}

	[HttpGet("error-message")]
	[ProducesResponseType(typeof(NodeErrorMessageDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetNodeErrorMessage(Guid dataFlowExecutionId, Guid nodeId,
		CancellationToken cancellationToken)
	{
		NodeErrorMessageDto result = await
			_nodeErrorMessageDtoProvider.GetNodeErrorMessageAsync(dataFlowExecutionId, nodeId, cancellationToken)
			.ConfigureAwait(false);
		return Ok(result);
	}
}
