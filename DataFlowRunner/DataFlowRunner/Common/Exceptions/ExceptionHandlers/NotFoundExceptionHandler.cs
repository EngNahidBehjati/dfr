using DataFlowRunner.Common.Exceptions.Abstraction;
using DataFlowRunner.Common.Exceptions.NotFound;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DataFlowRunner.Common.Exceptions.ExceptionHandlers;

public class NotFoundExceptionHandler : IExceptionHandler
{
	private readonly IProblemDetailsService _problemDetailsService;
	private readonly IStackTraceResolver _stackTraceResolver;

	public NotFoundExceptionHandler(IProblemDetailsService problemDetailsService,
		IStackTraceResolver stackTraceResolver)
	{
		_problemDetailsService = problemDetailsService ?? throw new ArgumentNullException(nameof(problemDetailsService));
		_stackTraceResolver = stackTraceResolver ?? throw new ArgumentNullException(nameof(stackTraceResolver));
	}

	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
		CancellationToken cancellationToken)
	{
		if (exception is not (DataFlowExecutionNotFoundException or EdgeNotFoundException or NodeNotFoundException))
		{
			return false;
		}

		var problemDetail = new ProblemDetails
		{
			Status = StatusCodes.Status404NotFound,
			Detail = _stackTraceResolver.ResolveStackTrace(exception),
			Title = exception.Message,
			Type = "Not Found"
		};
		return await _problemDetailsService.TryWriteAsync(
			new ProblemDetailsContext
			{
				HttpContext = httpContext,
				ProblemDetails = problemDetail
			}).ConfigureAwait(false);
	}
}
