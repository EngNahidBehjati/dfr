using DataFlowRunner.TableQuery.Reports;
using DataFlowRunner.TableQuery.WolverineIntegration.Abstractions;
using DataFlowRunner.TableQuery.WolverineIntegration.Messages;
using Riok.Mapperly.Abstractions;

namespace DataFlowRunner.TableQuery.WolverineIntegration.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal sealed partial class WolverineTableQueryReportMapper :
	IWolverineTableQueryReportMapper<ExecuteTableQueryWolverineReport, ExecuteTableQueryReport>,
	IWolverineTableQueryReportMapper<CalculateRowCountWolverineReport, CalculateRowCountReport>
{
	public partial ExecuteTableQueryReport ToReport(ExecuteTableQueryWolverineReport wolverineReport);

	public partial CalculateRowCountReport ToReport(CalculateRowCountWolverineReport wolverineReport);
}
