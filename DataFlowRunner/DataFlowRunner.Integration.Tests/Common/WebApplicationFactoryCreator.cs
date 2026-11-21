using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.Common.DataFlowBuilderConfiguration.Options;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Wolverine;

namespace DataFlowRunner.Integration.Tests.Common;

internal static class WebApplicationFactoryCreator
{
	public static WebApplicationFactory<Program> Create(
		string? dataFlowBuilderAddress = null,
		string? amAddress = null,
		bool skipDataFlowAuthorization = true)
	{
		return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
		{
			builder.UseSetting("OpenTelemetry:Enabled", "false");
			ConfigureServices(builder, dataFlowBuilderAddress, amAddress);
			ConfigureLogging(builder);
			if (skipDataFlowAuthorization)
			{
				SkipDataFlowExecutionAuthorization(builder);
			}
		});
	}

	private static void SkipDataFlowExecutionAuthorization(IWebHostBuilder builder)
	{
		builder.ConfigureTestServices(services =>
		{
			services.RemoveAll<IDataFlowExecutionAuthorizerClient>();
			services.AddSingleton(Substitute.For<IDataFlowExecutionAuthorizerClient>());
		});
	}

	private static void ConfigureServices(IWebHostBuilder builder, string? dataFlowBuilderAddress, string? amAddress)
	{
		builder.ConfigureTestServices(services =>
		{
			ConfigureDataFlowBuilder(services, dataFlowBuilderAddress);
			ConfigureAmConfig(services, amAddress);

			services.RemoveAll<IWolverineExtension>();
			services.AddSingleton<IWolverineExtension, TestsWolverineExtension>();
		});
	}

	private static void ConfigureDataFlowBuilder(IServiceCollection services, string? dataFlowBuilderAddress)
	{
		if (dataFlowBuilderAddress is null) return;

		services.Configure<DataFlowBuilderEndpoint>(options =>
		{
			options.BaseUrl = dataFlowBuilderAddress;
		});
	}

	private static void ConfigureAmConfig(IServiceCollection services, string? amAddress)
	{
		if (amAddress is null) return;

		services.Configure<AmConfig>(options =>
		{
			options.BaseUrl = new Uri(amAddress);
			options.EngineVersion = "test";
		});
	}

	private static void ConfigureLogging(IWebHostBuilder builder)
	{
		builder.ConfigureLogging(logging =>
		{
			logging.ClearProviders();
			logging.AddConsole();
		});
	}
}
