using DataFlowRunner.TableQuery.Models;

namespace DataFlowRunner.TableQuery.Abstractions;

public interface ITableQueryFacade
{
	Task<TableQueryResponseDto> QueryTableAsync(
		Guid dataFlowExecutionId,
		Guid nodeId,
		Guid outputPortId,
		bool includeSchema,
		IReadOnlyDictionary<string, string> queryParameters,
		CancellationToken cancellationToken);

	Task<long> GetRowCountAsync(
		Guid dataFlowExecutionId,
		Guid nodeId,
		Guid outputPortId,
		IReadOnlyDictionary<string, string> queryParameters,
		CancellationToken cancellationToken);
}
