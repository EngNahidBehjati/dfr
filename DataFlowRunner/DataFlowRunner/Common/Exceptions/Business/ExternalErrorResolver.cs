using System.Text.Json;
using DataFlowRunner.Common.Exceptions.Abstraction;
using DataFlowRunner.Common.Exceptions.Models;

namespace DataFlowRunner.Common.Exceptions.Business;

public sealed class ExternalErrorResolver : IExternalErrorResolver
{
	private static readonly JsonSerializerOptions _options = new()
	{
		PropertyNameCaseInsensitive = true
	};

	public ExternalErrorResponse ResolveError(string? content)
	{
		if (string.IsNullOrWhiteSpace(content))
			return new ExternalErrorResponse();

		try
		{
			return JsonSerializer.Deserialize<ExternalErrorResponse>(content, _options)
			       ?? new ExternalErrorResponse();
		}
		catch
		{
			return new ExternalErrorResponse();
		}
	}
}
