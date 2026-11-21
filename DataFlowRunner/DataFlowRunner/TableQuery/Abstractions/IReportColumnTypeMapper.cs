using DataFlowRunner.TableQuery.Models;
using DataFlowRunner.TableQuery.Reports;

namespace DataFlowRunner.TableQuery.Abstractions;

internal interface IReportColumnTypeMapper
{
	ColumnTypeDto MapColumnType(ReportColumnType reportType);
}
