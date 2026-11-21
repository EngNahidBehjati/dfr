using DataFlowRunner.TableQuery.Abstractions;

namespace DataFlowRunner.TableQuery.Business;

internal sealed class QueryParametersExtractor : IQueryParametersExtractor
{
	private const string IncludeSchemaQueryKey = "includeSchema";

	public IReadOnlyDictionary<string, string> GetParameters(IQueryCollection queryCollection)
	{
		return GetParametersExcluding(queryCollection, IncludeSchemaQueryKey);
	}

	private static Dictionary<string, string> GetParametersExcluding(IQueryCollection queryCollection, string excludedKey)
	{
		ArgumentNullException.ThrowIfNull(queryCollection);

		return queryCollection
			.Where(q => !string.Equals(q.Key, excludedKey, StringComparison.OrdinalIgnoreCase))
			.ToDictionary(q => q.Key, q => q.Value.ToString());
	}
}
