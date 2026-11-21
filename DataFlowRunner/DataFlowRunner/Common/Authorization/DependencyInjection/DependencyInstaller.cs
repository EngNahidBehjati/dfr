using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.Common.Authorization.Abstraction;
using DataFlowRunner.Common.Authorization.Business;
using DataFlowRunner.Common.Authorization.Models;
using DataFlowRunner.Common.Authorization.Options;
using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.Common.Http.Extensions;
using Microsoft.Extensions.Options;
using Refit;

namespace DataFlowRunner.Common.Authorization.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddSingleton<IAdminAuthorization, AdminAuthorization>();

		services.AddSingleton<IValidateOptions<DiaRegistrationConfigs>, DiaRegistrationConfigsOptionValidation>();
		services.AddOptions<DiaRegistrationConfigs>().Configure<IConfiguration>((diaRegistrationConfigs, configuration) =>
		{
			IConfigurationSection accessManagementSection = configuration.GetSection("AppSettings")
				.GetSection("AccessManagement");
			diaRegistrationConfigs.TargetSystemId = accessManagementSection.GetValue<Guid>("DiaTargetSystemId");

			diaRegistrationConfigs.AdminServiceId = Guid.Parse("1edd36a8-2dad-44a7-b572-4d5632ffd9e2");
			diaRegistrationConfigs.ManageServiceAccessLevelId = Guid.Parse("c62d7cbe-228d-4a78-9e7e-625b8b1fee7f");
		}).ValidateOnStart();
	}
}
