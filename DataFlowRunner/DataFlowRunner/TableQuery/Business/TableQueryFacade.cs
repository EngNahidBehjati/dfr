using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.TableQuery.Abstractions;
using DataFlowRunner.TableQuery.Commands;
using DataFlowRunner.TableQuery.Exceptions;
using DataFlowRunner.TableQuery.Models;
using DataFlowRunner.TableQuery.Reports;

namespace DataFlowRunner.TableQuery.Business;

internal sealed class TableQueryFacade : ITableQueryFacade
{
	private readonly IDataFlowExecutionUserIdProvider _dataFlowExecutionUserIdProvider;

	private readonly ITableQueryCommandInvoker<ExecuteTableQueryCommand, ExecuteTableQueryReport>
		_tableQueryCommandInvoker;

	private readonly ITableQueryCommandInvoker<CalculateRowCountCommand, CalculateRowCountReport>
		_rowCountQueryCommandInvoker;

	private readonly INodeExecutionStateValidator _nodeExecutionStateValidator;
	private readonly ITableQueryResponseCreator _tableQueryResponseCreator;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public TableQueryFacade(IDataFlowExecutionUserIdProvider dataFlowExecutionUserIdProvider,
		ITableQueryCommandInvoker<ExecuteTableQueryCommand, ExecuteTableQueryReport> commandInvoker,
		ITableQueryCommandInvoker<CalculateRowCountCommand, CalculateRowCountReport> rowCountQueryCommandInvoker,
		INodeExecutionStateValidator nodeExecutionStateValidator,
		ITableQueryResponseCreator tableQueryResponseCreator,
		ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_dataFlowExecutionUserIdProvider = dataFlowExecutionUserIdProvider ??
		                                   throw new ArgumentNullException(nameof(dataFlowExecutionUserIdProvider));
		_tableQueryCommandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
		_rowCountQueryCommandInvoker = rowCountQueryCommandInvoker ??
		                               throw new ArgumentNullException(nameof(rowCountQueryCommandInvoker));
		_nodeExecutionStateValidator = nodeExecutionStateValidator ??
		                               throw new ArgumentNullException(nameof(nodeExecutionStateValidator));
		_tableQueryResponseCreator = tableQueryResponseCreator ??
		                             throw new ArgumentNullException(nameof(tableQueryResponseCreator));
		_languageDictionaryProvider = languageDictionaryProvider ??
		                              throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public async Task<TableQueryResponseDto> QueryTableAsync(
		Guid dataFlowExecutionId,
		Guid nodeId,
		Guid outputPortId,
		bool includeSchema,
		IReadOnlyDictionary<string, string> queryParameters,
		CancellationToken cancellationToken)
	{
		ValidateNodeIsSucceeded(dataFlowExecutionId, nodeId);

		UserId userId = _dataFlowExecutionUserIdProvider.GetDataFlowExecutionUserId(
			new DataFlowExecutionId(dataFlowExecutionId));

		ExecuteTableQueryCommand command = new()
		{
			GraphExecutionId = dataFlowExecutionId,
			UserId = userId.Value,
			NodeId = nodeId,
			OutputPortId = outputPortId,
			IncludeSchema = includeSchema,
			ODataQuery = queryParameters
		};

		ExecuteTableQueryReport report = await _tableQueryCommandInvoker
			.InvokeQueryCommandAsync(command, cancellationToken)
			.ConfigureAwait(false);

		if (report.Rows is null)
		{
			var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage
				.DataFlowRunner.TableQuery.RowsNotProvided();

			throw new TableQueryExecutionFailedException(errorMessage);
		}

		return _tableQueryResponseCreator.CreateResponseDto(report.TableSchema, report.Rows!);
	}

	public async Task<long> GetRowCountAsync(
		Guid dataFlowExecutionId,
		Guid nodeId,
		Guid outputPortId,
		IReadOnlyDictionary<string, string> queryParameters,
		CancellationToken cancellationToken)
	{
		ValidateNodeIsSucceeded(dataFlowExecutionId, nodeId);

		UserId userId = _dataFlowExecutionUserIdProvider.GetDataFlowExecutionUserId(
			new DataFlowExecutionId(dataFlowExecutionId));

		CalculateRowCountCommand command = new()
		{
			GraphExecutionId = dataFlowExecutionId,
			UserId = userId.Value,
			NodeId = nodeId,
			OutputPortId = outputPortId,
			ODataQuery = queryParameters
		};

		CalculateRowCountReport report = await _rowCountQueryCommandInvoker
			.InvokeQueryCommandAsync(command, cancellationToken)
			.ConfigureAwait(false);

		if (!report.RowCount.HasValue)
		{
			var errorMessage = _languageDictionaryProvider.DataFlowRunnerLanguage.DataFlowRunner
				.TableQuery.RowCountNotProvided();
			throw new TableQueryExecutionFailedException(errorMessage);
		}

		return report.RowCount.Value;
	}

	private void ValidateNodeIsSucceeded(Guid dataFlowExecutionId, Guid nodeId)
	{
		_nodeExecutionStateValidator.ValidateNodeIsSucceeded(new DataFlowExecutionId(dataFlowExecutionId),
			new NodeId(nodeId));
	}
}
