using DataFlowRunner.Common.Authentication.Abstractions;
using DataFlowRunner.Common.Authentication.Business;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.Common.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DataFlowRunner.Common.Authentication.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddOptions<AmConfig>().Configure<IConfiguration>((amConfig, configuration) =>
		{
			IConfigurationSection amConfigSection = configuration.GetSection("AppSettings").GetSection("AccessManagement");
			amConfig.BaseUrl = new Uri(amConfigSection.GetValue("BaseUrl", string.Empty)!);
			amConfig.EngineVersion = amConfigSection.GetValue("EngineVersion", string.Empty)!;
			amConfig.EngineCertHash = amConfigSection.GetValue("EngineCertHash", string.Empty)!;
		}).ValidateOnStart();

		services.AddSingleton<IValidateOptions<AmConfig>, AmConfigValidator>();
		services.AddSingleton<IAuthenticationTokenAccessor, AuthenticationTokenAccessor>();

		services.AddSingleton<IAuthenticatedUserProvider, AuthenticatedUserProvider>();
	}
}
