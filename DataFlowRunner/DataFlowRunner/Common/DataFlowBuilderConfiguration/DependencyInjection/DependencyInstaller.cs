using DataFlowRunner.Common.DataFlowBuilderConfiguration.Options;
using DataFlowRunner.Common.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DataFlowRunner.Common.DataFlowBuilderConfiguration.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddSingleton<IValidateOptions<DataFlowBuilderEndpoint>, DataFlowBuilderEndpointOptionValidation>();
		services.AddOptions<DataFlowBuilderEndpoint>()
			.Configure<IConfiguration>((endpoint, configuration) =>
			{
				IConfigurationSection dataFlowBuilderSection = configuration.GetSection("AppSettings")
					.GetSection("DataFlowBuilder");
				endpoint.BaseUrl = dataFlowBuilderSection.GetValue("BaseUrl", string.Empty)!;
				endpoint.EngineCertHash = dataFlowBuilderSection.GetValue("EngineCertHash", string.Empty)!;
			}).ValidateOnStart();
	}
}
