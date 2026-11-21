using System.Diagnostics.CodeAnalysis;
using DataFlowRunner.Common.Exceptions.Abstraction;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DataFlowRunner.DataFlowAuthorization.Exceptions.ExceptionHandlers;

/// <summary>
/// Handles data flow execution authorization-related exceptions and maps them to appropriate HTTP status codes.
/// </summary>
public sealed class DataFlowExecutionAuthorizationExceptionHandler : IExceptionHandler
{
	private readonly IProblemDetailsService _problemDetailsService;
	private readonly IStackTraceResolver _stackTraceResolver;

	public DataFlowExecutionAuthorizationExceptionHandler(IProblemDetailsService problemDetailsService,
		IStackTraceResolver stackTraceResolver)
	{
		_problemDetailsService =
			problemDetailsService ?? throw new ArgumentNullException(nameof(problemDetailsService));
		_stackTraceResolver = stackTraceResolver ?? throw new ArgumentNullException(nameof(stackTraceResolver));
	}

	public async ValueTask<bool> TryHandleAsync(
		HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken)
	{
		if (!TryGetProblemDetails(exception, out ProblemDetails? problemDetails))
		{
			return false;
		}

		return await _problemDetailsService.TryWriteAsync(
			new ProblemDetailsContext
			{
				HttpContext = httpContext,
				ProblemDetails = problemDetails
			}).ConfigureAwait(false);
	}

	private bool TryGetProblemDetails(Exception exception,
		[NotNullWhen(true)] out ProblemDetails? problemDetails)
	{
		problemDetails = exception switch
		{
			DataFlowExecutionIdNotFoundInRouteException => CreateProblemDetails(exception, "BadRequest",
				StatusCodes.Status400BadRequest),
			DataFlowExecutionUnauthorizedException => CreateProblemDetails(exception, "Unauthorized",
				StatusCodes.Status401Unauthorized),
			DataFlowExecutionUserIdNotFoundException => CreateProblemDetails(exception, "Unauthorized",
				StatusCodes.Status401Unauthorized),
			DataFlowExecutionNotFoundOrAccessDeniedException => CreateProblemDetails(exception, "NotFound",
				StatusCodes.Status404NotFound),
			DataFlowExecutionUserIdConflictException => CreateProblemDetails(exception, "Conflict",
				StatusCodes.Status409Conflict),
			DataFlowExecutionAuthorizationFailedException => CreateProblemDetails(exception, "Internal Server Error",
				StatusCodes.Status500InternalServerError),
			_ => null
		};

		return problemDetails != null;
	}

	private ProblemDetails CreateProblemDetails(Exception exception, string exceptionType, int statusCode)
	{
		var problemDetail = new ProblemDetails
		{
			Status = statusCode,
			Detail = _stackTraceResolver.ResolveStackTrace(exception),
			Title = exception.Message,
			Type = exceptionType
		};
		return problemDetail;
	}
}
