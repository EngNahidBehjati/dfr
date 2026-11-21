using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.TableQuery.Abstractions;
using DataFlowRunner.TableQuery.Business;
using DataFlowRunner.TableQuery.Commands;
using DataFlowRunner.TableQuery.Mappers;
using DataFlowRunner.TableQuery.Options;
using DataFlowRunner.TableQuery.Reports;
using DataFlowRunner.TableQuery.WolverineIntegration.Abstractions;
using DataFlowRunner.TableQuery.WolverineIntegration.Mappers;
using DataFlowRunner.TableQuery.WolverineIntegration.Messages;
using Microsoft.Extensions.Options;

namespace DataFlowRunner.TableQuery.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddSingleton<
			ITableQueryCommandInvoker<ExecuteTableQueryCommand, ExecuteTableQueryReport>,
			WolverineTableQueryCommandInvoker<ExecuteTableQueryCommand, ExecuteTableQueryReport,
				ExecuteTableQueryWolverineReport>>();
		services.AddSingleton<
			ITableQueryCommandInvoker<CalculateRowCountCommand, CalculateRowCountReport>,
			WolverineTableQueryCommandInvoker<CalculateRowCountCommand, CalculateRowCountReport,
				CalculateRowCountWolverineReport>>();

		services.AddSingleton<IWolverineTableQueryCommandMapper, WolverineTableQueryCommandMapper>();

		services.AddSingleton<WolverineTableQueryReportMapper>();
		services.AddSingleton<
			IWolverineTableQueryReportMapper<ExecuteTableQueryWolverineReport, ExecuteTableQueryReport>>(
			serviceProvider => serviceProvider.GetRequiredService<WolverineTableQueryReportMapper>());
		services.AddSingleton<
			IWolverineTableQueryReportMapper<CalculateRowCountWolverineReport, CalculateRowCountReport>>(
			serviceProvider => serviceProvider.GetRequiredService<WolverineTableQueryReportMapper>());

		services.AddSingleton<ITableQueryFacade, TableQueryFacade>();
		services.AddSingleton<IReportColumnTypeMapper, ReportColumnTypeMapper>();
		services.AddSingleton<ITableQueryResponseCreator, TableQueryResponseCreator>();
		services.AddSingleton<INodeExecutionStateValidator, NodeExecutionStateValidator>();
		services.AddSingleton<IQueryParametersExtractor, QueryParametersExtractor>();

		services.AddSingleton<IValidateOptions<TableQueryInvokeTimeoutOptions>, TableQueryInvokeTimeoutOptionValidation>();
		services.AddOptions<TableQueryInvokeTimeoutOptions>().Configure<IConfiguration>((tableQueryInvokeTimeoutOptions, configuration) =>
		{
			IConfigurationSection nodeExecutorEngineSection = configuration.GetSection("AppSettings").GetSection("NodeExecutorEngine");
			var timeoutInSeconds = nodeExecutorEngineSection.GetValue("TableQueryInvokeTimeoutInSeconds", 300);
			tableQueryInvokeTimeoutOptions.TimeoutInSeconds = TimeSpan.FromSeconds(timeoutInSeconds);
		}).ValidateOnStart();
	}
}
