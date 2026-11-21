using DataFlowRunner.Common.Exceptions.Abstraction;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DataFlowRunner.TableQuery.Exceptions.ExceptionHandlers;

internal sealed class TableQueryExceptionHandler : IExceptionHandler
{
	private readonly IProblemDetailsService _problemDetailsService;
	private readonly IStackTraceResolver _stackTraceResolver;

	public TableQueryExceptionHandler(IProblemDetailsService problemDetailsService,
		IStackTraceResolver stackTraceResolver)
	{
		_problemDetailsService = problemDetailsService ?? throw new ArgumentNullException(nameof(problemDetailsService));
		_stackTraceResolver = stackTraceResolver ?? throw new ArgumentNullException(nameof(stackTraceResolver));
	}

	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
	{
		ProblemDetails? problemDetail = exception switch
		{
			NodeExecutionStateNotSucceededException =>
				CreateProblemDetails(StatusCodes.Status409Conflict, "Conflict", exception),
			TableQueryExecutionFailedException =>
				CreateProblemDetails(StatusCodes.Status500InternalServerError, "TableQueryExecutionFailed", exception),
			_ => null
		};

		if (problemDetail is null)
		{
			return false;
		}

		return await _problemDetailsService.TryWriteAsync(
			new ProblemDetailsContext
			{
				HttpContext = httpContext,
				ProblemDetails = problemDetail
			}).ConfigureAwait(false);
	}

	private ProblemDetails CreateProblemDetails(
		int statusCode,
		string type,
		Exception exception)
	{
		return new ProblemDetails
		{
			Status = statusCode,
			Detail = _stackTraceResolver.ResolveStackTrace(exception),
			Title = exception.Message,
			Type = type
		};
	}
}
