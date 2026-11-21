using DataFlowRunner.Common.DataFlowBuilderConfiguration.Options;
using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.Common.Http.Extensions;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Infrastructure;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.ProvideRequiredDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Store;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.RunDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.Validation;
using DataFlowRunner.DataFlowDynamicParameters.Business.Infrastructure;
using DataFlowRunner.DataFlowDynamicParameters.Business.ProvideRequiredDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Business.RunDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Business.Store;
using DataFlowRunner.DataFlowDynamicParameters.Business.Validation;
using DataFlowRunner.DataFlowExecution.Abstractions;
using Microsoft.Extensions.Options;
using Refit;

namespace DataFlowRunner.DataFlowDynamicParameters.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddSingleton<IDataFlowDynamicParametersStore, DataFlowDynamicParametersStore>();
		services.AddSingleton<IDataFlowExecutionSequenceStore, DataFlowExecutionSequenceStore>();
		services.AddSingleton<INodeAncestryParameterValueExtractor, NodeAncestryParameterValueExtractor>();
		services.AddSingleton<IDataFlowParametersValueExtractor, DataFlowParametersValueExtractor>();
		services.AddSingleton<IDynamicParameterJsonValueConverter, DynamicParameterJsonValueConverter>();
		services.AddSingleton<IDynamicParameterTypeValidator, DynamicParameterTypeValidator>();
		services.AddSingleton<IDynamicParametersResponseResolver, DynamicParametersResponseResolver>();
		services.AddSingleton<RequiredDynamicParametersProvider>();
		services.AddSingleton<IRequiredDynamicParametersProvider>(serviceProvider =>
		{
			RequiredDynamicParametersProvider inner = serviceProvider.GetRequiredService<RequiredDynamicParametersProvider>();
			IDataFlowVersionConflictHandler versionConflictHandler =
				serviceProvider.GetRequiredService<IDataFlowVersionConflictHandler>();
			return new VersionSynchronizingRequiredDynamicParametersProvider(inner, versionConflictHandler);
		});

		services.AddSingleton<IDynamicParametersStoreUpdater, DynamicParametersStoreUpdater>();
		services.AddSingleton<IDynamicParametersFetcher, DynamicParametersFetcher>();
		services.AddSingleton<IRunDynamicParametersValidator, RunDynamicParametersValidator>();
		services.AddSingleton<IRunDynamicParametersByNodeResolver, RunDynamicParametersByNodeResolver>();

		services.AddRefitClient<IDataFlowDynamicParametersClientApi>()
			.ConfigureHttpClient((serviceProvider, client) =>
			{
				DataFlowBuilderEndpoint endpoint =
					serviceProvider.GetRequiredService<IOptions<DataFlowBuilderEndpoint>>().Value;
				client.BaseAddress = new Uri(endpoint.BaseUrl);
			})
			.AddDataFlowBuilderCertificateValidation();
	}
}
