using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.GraphCore.Business;

namespace DataFlowRunner.GraphCore.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddSingleton<IGraphFactory, GraphFactory>();
	}
}
