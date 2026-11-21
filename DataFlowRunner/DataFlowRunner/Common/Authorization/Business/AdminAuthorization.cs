using DataFlowRunner.ApplicationInformation.Abstraction;
using DataFlowRunner.Common.Authentication.Abstractions;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.Common.Authorization.Abstraction;
using DataFlowRunner.Common.Authorization.Models;
using Microsoft.Extensions.Options;

namespace DataFlowRunner.Common.Authorization.Business;

internal sealed class AdminAuthorization : IAdminAuthorization
{
	private readonly IOptions<DiaRegistrationConfigs> _diaRegistrationConfigs;
	private readonly IAuthenticationTokenAccessor _authenticationTokenAccessor;
	private readonly IApplicationVersionGetter _applicationVersionGetter;

	public AdminAuthorization(IOptions<DiaRegistrationConfigs> diaRegistrationConfigs,
		IAuthenticationTokenAccessor authenticationTokenAccessor,
		IApplicationVersionGetter applicationVersionGetter)
	{
		_diaRegistrationConfigs =
			diaRegistrationConfigs ?? throw new ArgumentNullException(nameof(diaRegistrationConfigs));

		_authenticationTokenAccessor = authenticationTokenAccessor ??
		                               throw new ArgumentNullException(nameof(authenticationTokenAccessor));
		_applicationVersionGetter = applicationVersionGetter ??
		                            throw new ArgumentNullException(nameof(applicationVersionGetter));
	}

	public async Task<bool> UserIsAdmin()
	{
		return true;
	}
}
