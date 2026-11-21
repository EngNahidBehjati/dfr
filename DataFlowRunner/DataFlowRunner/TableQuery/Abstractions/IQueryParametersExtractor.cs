namespace DataFlowRunner.TableQuery.Abstractions;

public interface IQueryParametersExtractor
{
	IReadOnlyDictionary<string, string> GetParameters(IQueryCollection queryCollection);
}
