using DataFlowRunner.ApplicationInformation.Abstraction;
using DataFlowRunner.ApplicationInformation.Business;
using DataFlowRunner.Common.DependencyInjection;

namespace DataFlowRunner.ApplicationInformation.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddSingleton<IApplicationVersionGetter, ApplicationVersionGetter>();
	}
}
