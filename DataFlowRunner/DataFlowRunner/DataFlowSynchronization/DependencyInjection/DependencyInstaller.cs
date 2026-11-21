using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.DataFlowSynchronization.Abstraction;
using DataFlowRunner.DataFlowSynchronization.Business;

namespace DataFlowRunner.DataFlowSynchronization.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddSingleton<ISemaphoreStore, SemaphoreStore>();
		services.AddSingleton<IDataFlowActionLockExecutor, DataFlowActionLockExecutor>();
	}
}
