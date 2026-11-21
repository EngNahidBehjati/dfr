using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.Common.Exceptions.Abstraction;
using DataFlowRunner.Common.Exceptions.Business;

namespace DataFlowRunner.Common.Exceptions.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddSingleton<IStackTraceResolver, StackTraceResolver>();
		services.AddSingleton<IExternalErrorResolver, ExternalErrorResolver>();
	}
}
