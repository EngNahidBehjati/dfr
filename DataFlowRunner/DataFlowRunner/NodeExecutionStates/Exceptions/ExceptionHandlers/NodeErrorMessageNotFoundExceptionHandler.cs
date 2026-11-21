using System.Diagnostics.CodeAnalysis;
using DataFlowRunner.Common.Exceptions.Abstraction;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DataFlowRunner.NodeExecutionStates.Exceptions.ExceptionHandlers;

internal sealed class NodeErrorMessageNotFoundExceptionHandler : IExceptionHandler
{
	private readonly IProblemDetailsService _problemDetailsService;
	private readonly IStackTraceResolver _stackTraceResolver;

	public NodeErrorMessageNotFoundExceptionHandler(IProblemDetailsService problemDetailsService,
		IStackTraceResolver stackTraceResolver)
	{
		_problemDetailsService = problemDetailsService ?? throw new ArgumentNullException(nameof(problemDetailsService));
		_stackTraceResolver = stackTraceResolver ?? throw new ArgumentNullException(nameof(stackTraceResolver));
	}

	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
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
			InvalidNodeExecutionStateException => CreateProblemDetails(exception, "Internal Server Error",
				StatusCodes.Status500InternalServerError),
			NodeExecutionStateTransitionNotAllowedException => CreateProblemDetails(exception, "Conflict",
				StatusCodes.Status409Conflict),
			NodeErrorMessageNotFoundException => CreateProblemDetails(exception, "Not Found",
				StatusCodes.Status404NotFound),
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
