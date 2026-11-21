using DataFlowRunner.Common.DataFlowBuilderConfiguration.Options;
using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.Common.Http.Extensions;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowAuthorization.Business;
using Microsoft.Extensions.Options;
using Refit;

namespace DataFlowRunner.DataFlowAuthorization.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddSingleton<IDataFlowExecutionUserIdStore, DataFlowExecutionUserIdStore>();
		services.AddSingleton<IDataFlowExecutionUserIdProvider>(provider => provider.GetRequiredService<IDataFlowExecutionUserIdStore>());
		services.AddSingleton<IRouteParameterExtractor, RouteParameterExtractor>();
		services.AddSingleton<IDataFlowExecutionAuthorizerClient, DataFlowExecutionAuthorizerClient>();
		services.AddSingleton<IDataFlowExecutionUserAuthorizer, DataFlowExecutionUserAuthorizer>();

		services.AddRefitClient<IDataFlowAuthorizationClientApi>()
			.ConfigureHttpClient((serviceProvider, client) =>
			{
				DataFlowBuilderEndpoint endpoint =
					serviceProvider.GetRequiredService<IOptions<DataFlowBuilderEndpoint>>().Value;
				client.BaseAddress = new Uri(endpoint.BaseUrl);
			})
			.AddDataFlowBuilderCertificateValidation();
	}
}
