using DataFlowRunner.Common.DependencyInjection;

namespace DataFlowRunner.Common.Configuration.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddOptions<DataFlowExecutionFeatureFlags>()
			.Configure<IConfiguration>((featureFlags, configuration) =>
				configuration.GetSection("AppSettings:FeatureFlags").Bind(featureFlags));
	}
}
