using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.DataFlowExecutionEvents.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Mappers;

namespace DataFlowRunner.DataFlowExecutionEvents.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddScoped<IDataFlowExecutionCommandPublisher, WolverineDataFlowExecutionCommandPublisher>();
		services.AddSingleton<IWolverineExecutionCommandMapper, WolverineExecutionCommandMapper>();
		services.AddSingleton<IWolverineExecutionReportMapper, WolverineExecutionReportMapper>();
	}
}
