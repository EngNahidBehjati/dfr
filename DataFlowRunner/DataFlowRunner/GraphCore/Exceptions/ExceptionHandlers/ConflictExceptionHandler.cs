using DataFlowRunner.Common.Exceptions.Abstraction;
using DataFlowRunner.GraphCore.Exceptions.Conflict;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DataFlowRunner.GraphCore.Exceptions.ExceptionHandlers;

public class ConflictExceptionHandler : IExceptionHandler
{
	private readonly IProblemDetailsService _problemDetailsService;
	private readonly IStackTraceResolver _stackTraceResolver;

	public ConflictExceptionHandler(IProblemDetailsService problemDetailsService, IStackTraceResolver stackTraceResolver)
	{
		_problemDetailsService = problemDetailsService ?? throw new ArgumentNullException(nameof(problemDetailsService));
		_stackTraceResolver = stackTraceResolver ?? throw new ArgumentNullException(nameof(stackTraceResolver));
	}

	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken)
	{
		if (exception is not (DuplicateEdgeException or DuplicateNodeException))
		{
			return false;
		}

		var problemDetail = new ProblemDetails
		{
			Status = StatusCodes.Status409Conflict,
			Detail = _stackTraceResolver.ResolveStackTrace(exception),
			Title = exception.Message,
			Type = "Conflict"
		};

		return await _problemDetailsService.TryWriteAsync(
			new ProblemDetailsContext
			{
				HttpContext = httpContext,
				ProblemDetails = problemDetail
			}).ConfigureAwait(false);
	}
}
