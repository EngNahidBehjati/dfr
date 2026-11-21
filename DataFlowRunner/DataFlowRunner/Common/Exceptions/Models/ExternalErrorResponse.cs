namespace DataFlowRunner.Common.Exceptions.Models;

public sealed class ExternalErrorResponse
{
	public string? Message { get; init; }
	public string? StackTrace { get; init; }
}
