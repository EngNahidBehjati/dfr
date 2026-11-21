using DataFlowRunner.Common.Exceptions.Abstraction;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DataFlowRunner.DataFlowDynamicParameters.Exceptions.ExceptionHandlers;

public sealed class DynamicParameterNotFilledExceptionHandler: IExceptionHandler
{
	private readonly IProblemDetailsService _problemDetailsService;
	private readonly IStackTraceResolver _stackTraceResolver;

	public DynamicParameterNotFilledExceptionHandler(IProblemDetailsService problemDetailsService,
		IStackTraceResolver stackTraceResolver)
	{
		_problemDetailsService = problemDetailsService ?? throw new ArgumentNullException(nameof(problemDetailsService));
		_stackTraceResolver = stackTraceResolver ?? throw new ArgumentNullException(nameof(stackTraceResolver));
	}

	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
		CancellationToken cancellationToken)
	{
		if (exception is not DynamicParameterNotFilledException)
		{
			return false;
		}

		var problemDetail = new ProblemDetails
		{
			Status = StatusCodes.Status400BadRequest,
			Detail = _stackTraceResolver.ResolveStackTrace(exception),
			Title = exception.Message,
			Type = "Bad Request"
		};
		return await _problemDetailsService.TryWriteAsync(
			new ProblemDetailsContext
			{
				HttpContext = httpContext,
				ProblemDetails = problemDetail
			}).ConfigureAwait(false);
	}
}
