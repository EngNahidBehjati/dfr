using DataFlowRunner.TableQuery.Abstractions;

namespace DataFlowRunner.TableQuery.WolverineIntegration.Abstractions;

internal interface IWolverineTableQueryReportMapper<in TWolverineReport, out TReport>
	where TWolverineReport : class
	where TReport : IQueryReport
{
	TReport ToReport(TWolverineReport wolverineReport);
}
