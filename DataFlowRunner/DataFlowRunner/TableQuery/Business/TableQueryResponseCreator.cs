using DataFlowRunner.TableQuery.Abstractions;
using DataFlowRunner.TableQuery.Models;
using DataFlowRunner.TableQuery.Reports;

namespace DataFlowRunner.TableQuery.Business;

internal sealed class TableQueryResponseCreator : ITableQueryResponseCreator
{
	private readonly IReportColumnTypeMapper _reportColumnTypeMapper;

	public TableQueryResponseCreator(IReportColumnTypeMapper reportColumnTypeMapper)
	{
		_reportColumnTypeMapper = reportColumnTypeMapper ?? throw new ArgumentNullException(nameof(reportColumnTypeMapper));
	}

	public TableQueryResponseDto CreateResponseDto(IReadOnlyList<TableQueryReportColumn>? tableQueryReportColumns,
		IReadOnlyList<TableQueryReportRow> tableQueryReportRows)
	{
		ArgumentNullException.ThrowIfNull(tableQueryReportRows);

		IReadOnlyList<TableColumnDto>? tableSchema = tableQueryReportColumns?.Select(reportColumn => new TableColumnDto
		{
			Name = reportColumn.Name,
			DisplayName = reportColumn.DisplayName,
			DataType = _reportColumnTypeMapper.MapColumnType(reportColumn.DataType)
		}).ToList();

		IReadOnlyList<RowDto> rows = tableQueryReportRows.Select(row => new RowDto
		{
			Cells = row.Cells
		}).ToList();

		return new TableQueryResponseDto
		{
			TableSchema = tableSchema,
			Rows = rows
		};
	}
}
