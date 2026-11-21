using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Asp.Versioning;
using DataFlowRunner.Common.ApiConfiguration;
using DataFlowRunner.DataFlowAuthorization.Attributes;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Dto;
using Microsoft.AspNetCore.Mvc;

namespace DataFlowRunner.DataFlowExecution.Controllers;

[ApiController]
[Route($"{ApiRoutes.Base}/{ApiRoutes.DataFlowExecutions}/{{dataFlowExecutionId:guid}}")]
[ApiVersion("1")]
[AuthorizeDataFlowExecution]
public sealed class DataFlowExecutionController : ControllerBase
{
	private readonly IDataFlowExecutionFacade _dataFlowExecutionFacade;
	private readonly ILogger<DataFlowExecutionController> _logger;

	public DataFlowExecutionController(IDataFlowExecutionFacade dataFlowExecutionFacade,
		ILogger<DataFlowExecutionController> logger)
	{
		_dataFlowExecutionFacade =
			dataFlowExecutionFacade ?? throw new ArgumentNullException(nameof(dataFlowExecutionFacade));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	[HttpPost("run")]
	public async Task<IActionResult> RunDataFlow(Guid dataFlowExecutionId,
		[Required] long dataFlowVersion,
		[FromBody] RunDataFlowDto? requestBodyDto,
		CancellationToken cancellationToken)
	{
		using IDisposable? scope = _logger.BeginScope(new Dictionary<string, object>
		{
			["ExecutionId"] = dataFlowExecutionId,
			["Version"] = dataFlowVersion,
			["RequestBodyDto"] = JsonSerializer.Serialize(requestBodyDto)
		});
		try
		{
			_logger.LogInformation("RunDataFlow started");
			requestBodyDto ??= new RunDataFlowDto();
			await _dataFlowExecutionFacade
				.RunDataFlow(dataFlowExecutionId, dataFlowVersion, requestBodyDto, cancellationToken)
				.ConfigureAwait(false);
			_logger.LogInformation("RunDataFlow completed");
		}
		catch (Exception e)
		{
			_logger.LogError(e, "RunDataFlow failed");
			throw;
		}

		return Ok();
	}

	[HttpPost("run/{nodeId}")]
	public async Task<IActionResult> RunDataFlowNode(Guid dataFlowExecutionId,
		Guid nodeId,
		[Required] long dataFlowVersion,
		[FromBody] RunDataFlowNodeDto? requestBodyDto,
		CancellationToken cancellationToken)
	{
		using IDisposable? scope = _logger.BeginScope(new Dictionary<string, object>
		{
			["ExecutionId"] = dataFlowExecutionId,
			["NodeId"] = nodeId,
			["Version"] = dataFlowVersion,
			["RequestBodyDto"] = JsonSerializer.Serialize(requestBodyDto)
		});
		try
		{
			_logger.LogInformation("RunDataFlowNode started");
			requestBodyDto ??= new RunDataFlowNodeDto();
			await _dataFlowExecutionFacade
				.RunDataFlowNode(dataFlowExecutionId, nodeId, dataFlowVersion, requestBodyDto, cancellationToken)
				.ConfigureAwait(false);
			_logger.LogInformation("RunDataFlowNode completed");
		}
		catch (Exception e)
		{
			_logger.LogError(e, "RunDataFlowNode failed");
			throw;
		}

		return Ok();
	}

	[HttpPost("cancel")]
	public async Task<IActionResult> CancelDataFlow(Guid dataFlowExecutionId, CancellationToken cancellationToken)
	{
		using IDisposable? scope = _logger.BeginScope(new Dictionary<string, object>
		{
			["ExecutionId"] = dataFlowExecutionId
		});
		try
		{
			_logger.LogInformation("CancelDataFlow started");
			await _dataFlowExecutionFacade.CancelDataFlow(dataFlowExecutionId, cancellationToken)
				.ConfigureAwait(false);
			_logger.LogInformation("CancelDataFlow completed");
		}
		catch (Exception e)
		{
			_logger.LogError(e, "CancelDataFlow failed");
			throw;
		}

		return Ok();
	}

	[HttpPost("cancel/{nodeId}")]
	public async Task<IActionResult> CancelDataFlowNode(Guid dataFlowExecutionId, Guid nodeId,
		CancellationToken cancellationToken)
	{
		using IDisposable? scope = _logger.BeginScope(new Dictionary<string, object>
		{
			["ExecutionId"] = dataFlowExecutionId,
			["NodeId"] = nodeId
		});
		try
		{
			_logger.LogInformation("CancelDataFlowNode started");
			await _dataFlowExecutionFacade
				.CancelDataFlowNode(dataFlowExecutionId, nodeId, cancellationToken)
				.ConfigureAwait(false);
			_logger.LogInformation("CancelDataFlowNode completed");
		}
		catch (Exception e)
		{
			_logger.LogError(e, "CancelDataFlowNode failed");
			throw;
		}

		return Ok();
	}

	[HttpPost("invalidate")]
	public async Task<IActionResult> InvalidateDataFlow(Guid dataFlowExecutionId, CancellationToken cancellationToken)
	{
		using IDisposable? scope = _logger.BeginScope(new Dictionary<string, object>
		{
			["ExecutionId"] = dataFlowExecutionId
		});
		try
		{
			_logger.LogInformation("InvalidateDataFlow started");
			await _dataFlowExecutionFacade.InvalidateDataFlow(dataFlowExecutionId, cancellationToken)
				.ConfigureAwait(false);
			_logger.LogInformation("InvalidateDataFlow completed");
		}
		catch (Exception e)
		{
			_logger.LogError(e, "InvalidateDataFlow failed");
			throw;
		}

		return Ok();
	}

	[HttpPost("invalidate/{nodeId}")]
	public async Task<IActionResult> InvalidateDataFlowNode(Guid dataFlowExecutionId, Guid nodeId,
		CancellationToken cancellationToken)
	{
		using IDisposable? scope = _logger.BeginScope(new Dictionary<string, object>
		{
			["ExecutionId"] = dataFlowExecutionId,
			["NodeId"] = nodeId
		});
		try
		{
			_logger.LogInformation("InvalidateDataFlowNode started");
			await _dataFlowExecutionFacade
				.InvalidateDataFlowNode(dataFlowExecutionId, nodeId, cancellationToken)
				.ConfigureAwait(false);
			_logger.LogInformation("InvalidateDataFlowNode completed");
		}
		catch (Exception e)
		{
			_logger.LogError(e, "InvalidateDataFlowNode failed");
			throw;
		}

		return Ok();
	}
}
