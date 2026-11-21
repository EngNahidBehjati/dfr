using DataFlowRunner.TableQuery.Models;
using DataFlowRunner.TableQuery.Reports;

namespace DataFlowRunner.TableQuery.Abstractions;

internal interface ITableQueryResponseCreator
{
	TableQueryResponseDto CreateResponseDto(IReadOnlyList<TableQueryReportColumn>? tableQueryReportColumns,
		IReadOnlyList<TableQueryReportRow> tableQueryReportRows);
}
