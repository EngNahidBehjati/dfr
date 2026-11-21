using System.Text.Json.Serialization;
using DataFlowRunner.Common.Authentication.Exceptions.ExceptionHandlers;
using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.Common.Exceptions.ExceptionHandlers;
using DataFlowRunner.Common.HealthChecks;
using DataFlowRunner.Common.Logging;
using DataFlowRunner.Common.OpenTelemetry;
using DataFlowRunner.Common.ProblemDetailsConfiguration;
using DataFlowRunner.Common.SwaggerConfiguration;
using DataFlowRunner.DataFlowAuthorization.Exceptions.ExceptionHandlers;
using DataFlowRunner.DataFlowDynamicParameters.Exceptions.ExceptionHandlers;
using DataFlowRunner.DataFlowChangeEvents.Business.WolverineIntegration;
using DataFlowRunner.DataFlowExecution.Exceptions.ExceptionHandlers;
using DataFlowRunner.ExecutionManagement.Exceptions.ExceptionHandlers;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.NodeExecutionStates.Exceptions.ExceptionHandlers;
using DataFlowRunner.TableQuery.Exceptions.ExceptionHandlers;
using Mohaymen.Localization.Asp.Extensions;
using JasperFx.Resources;
using Wolverine;

namespace DataFlowRunner;

public sealed class Program
{
	[STAThread]
	public static void Main(string[] args)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

		builder.AddLocalization(builder.Configuration);
		builder.ConfigureLogging();
		builder.ConfigureOpenTelemetry();

		builder.Services.AddWolverine(ExtensionDiscovery.ManualOnly,
			opts =>
			{
				opts.Discovery.DisableConventionalDiscovery();
				opts.Discovery.IncludeType<WolverineNodeExecutionReportsHandler>();
				opts.Discovery.IncludeType<WolverineDataFlowChangeEventsHandler>();
				opts.Services.AddResourceSetupOnStartup();
			});
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerWithAuthToken();

		builder.Services.AddCustomProblemDetails();
		builder.Services.AddSingleton<ILanguageDictionaryProvider>(_ => LanguageDictionaryProvider.Instance);
		builder.Services.AddExceptionHandler<ConflictExceptionHandler>();
		builder.Services.AddExceptionHandler<NodeErrorMessageNotFoundExceptionHandler>();
		builder.Services.AddExceptionHandler<TableQueryExceptionHandler>();
		builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
		builder.Services.AddExceptionHandler<DataFlowExecutionAuthorizationExceptionHandler>();
		builder.Services.AddExceptionHandler<AuthenticationExceptionHandler>();
		builder.Services.AddExceptionHandler<DynamicParameterNotFilledExceptionHandler>();
		builder.Services.AddExceptionHandler<RetryExecutionFailedExceptionHandler>();
		builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

		builder.Services.AddHttpClient();

		builder.Services.AddHealthChecks()
			.AddCheck<DataFlowBuilderHealthCheck>("dataFlowBuilder")
			.AddCheck<KafkaHealthCheck>("kafka");

		builder.Services.AddHttpContextAccessor();
		builder.Services.AddControllers().AddJsonOptions(x =>
		{
			x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
		});

		builder.Services.RegisterDependencies();

		WebApplication app = builder.Build();

		app.UseRequestLocalization();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();
		app.UseStatusCodePages();
		app.UseExceptionHandler();
		app.MapHealthChecks();
		app.UseAuthorization();
		app.MapControllers();

		app.Run();
	}
}
