using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.DataFlowCleanup.Abstraction;
using DataFlowRunner.DataFlowCleanup.Business;

namespace DataFlowRunner.DataFlowCleanup.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddSingleton<INodeRemover, NodeRemover>();
	}
}
