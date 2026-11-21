using DataFlowRunner.Common.Exceptions.Models;

namespace DataFlowRunner.Common.Exceptions.Abstraction;

public interface IExternalErrorResolver
{
	ExternalErrorResponse ResolveError(string? content);
}
