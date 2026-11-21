using DataFlowRunner.TableQuery.Abstractions;
using DataFlowRunner.TableQuery.Models;
using DataFlowRunner.TableQuery.Reports;
using Riok.Mapperly.Abstractions;

namespace DataFlowRunner.TableQuery.Mappers;

[Mapper]
internal sealed partial class ReportColumnTypeMapper : IReportColumnTypeMapper
{
	public partial ColumnTypeDto MapColumnType(ReportColumnType reportType);
}
