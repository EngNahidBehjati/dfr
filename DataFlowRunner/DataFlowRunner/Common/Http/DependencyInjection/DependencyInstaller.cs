using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.Common.Http.Constants;
using DataFlowRunner.Common.Http.Extensions;

namespace DataFlowRunner.Common.Http.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddHttpClient(HttpClientNames.AccessManagement)
			.AddAccessManagementCertificateValidation();
		services.AddHttpClient(HttpClientNames.DataFlowBuilder)
			.AddDataFlowBuilderCertificateValidation();
	}
}
