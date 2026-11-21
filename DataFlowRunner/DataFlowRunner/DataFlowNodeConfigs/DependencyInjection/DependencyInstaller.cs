using DataFlowRunner.Common.DataFlowBuilderConfiguration.Options;
using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.Common.Exceptions;
using DataFlowRunner.Common.Http.Extensions;
using DataFlowRunner.Common.Exceptions.Abstraction;
using DataFlowRunner.Common.Exceptions.Models;
using DataFlowRunner.DataFlowNodeConfigs.Abstractions;
using DataFlowRunner.DataFlowNodeConfigs.Business;
using DataFlowRunner.Language.LanguageDictionary;
using Microsoft.Extensions.Options;
using Refit;

namespace DataFlowRunner.DataFlowNodeConfigs.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddSingleton<IDataFlowNodeConfigsStore, InMemoryDataFlowNodeConfigsStore>();
		services.AddRefitClient<INodeConfigRetrieveClientApi>(provider =>
			{
				return new RefitSettings
				{
					ExceptionFactory = async response =>
					{
						if (response.IsSuccessStatusCode)
							return null;

						var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
						IExternalErrorResolver externalErrorResolver =
							provider.GetRequiredService<IExternalErrorResolver>();

						ExternalErrorResponse externalError = externalErrorResolver.ResolveError(content);
						var message = LanguageDictionaryProvider.Instance.DataFlowRunnerLanguage.DataFlowRunner
							.DataFlowNodeConfigs.GetPluginConfigsFail();

						return new DataFlowBuilderException(message, externalError);
					}
				};
			})
			.ConfigureHttpClient((serviceProvider, client) =>
			{
				DataFlowBuilderEndpoint endpoint =
					serviceProvider.GetRequiredService<IOptions<DataFlowBuilderEndpoint>>().Value;
				client.BaseAddress = new Uri(endpoint.BaseUrl);
			})
			.AddDataFlowBuilderCertificateValidation();
	}
}
