using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.ExecutionManagement.Abstractions;
using DataFlowRunner.ExecutionManagement.Business;
using DataFlowRunner.ExecutionManagement.Options;
using DataFlowRunner.ExecutionManagement.WolverineIntegration;
using Microsoft.Extensions.Options;

namespace DataFlowRunner.ExecutionManagement.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddScoped<IExecutionCanceller, ExecutionCanceller>();
		services.AddScoped<IExecutionRetrier, ExecutionRetrier>();
		services.AddScoped<ICancelExecutionCommandPublisher, WolverineExecutionCancelCommandPublisher>();
		services.AddScoped<IRetryExecutionCommandInvoker, WolverineRetryExecutionCommandInvoker>();

		services.AddSingleton<IValidateOptions<RetryExecutionInvokeTimeoutOptions>, RetryExecutionInvokeTimeoutOptionValidator>();
		services.AddOptions<RetryExecutionInvokeTimeoutOptions>().Configure<IConfiguration>((options, configuration) =>
		{
			IConfigurationSection nodeExecutorEngineSection =
				configuration.GetSection("AppSettings").GetSection("NodeExecutorEngine");
			var timeoutInSeconds =
				nodeExecutorEngineSection.GetValue(RetryExecutionInvokeTimeoutOptions.TimeoutKey, 120);
			options.TimeoutInSeconds = TimeSpan.FromSeconds(timeoutInSeconds);
		}).ValidateOnStart();
	}
}
