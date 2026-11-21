using JasperFx.Resources;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DataFlowRunner.Tests.DependencyInjection;

public class ServiceProviderCreationTests
{
	[Fact]
	public void ServiceProviderCreation_ShouldNotThrowException_WhenServiceProviderCreated()
	{
		// Arrange
		WebApplicationFactory<Program> webApplicationFactory = new WebApplicationFactory<Program>()
			.WithWebHostBuilder(builder =>
			{
				builder.UseSetting("OpenTelemetry:Enabled", "false");
				builder.ConfigureTestServices(RemoveWolverineResourceSetupHostService);
			});

		// Act
		Func<IServiceProvider> act = () => webApplicationFactory.Services;

		// Arrange
		act.Should().NotThrow();
	}

	private static void RemoveWolverineResourceSetupHostService(IServiceCollection services)
	{
		var resourceSetupHostServiceDescriptors = services
			.Where(d =>
				d.ServiceType == typeof(IHostedService) &&
				d.ImplementationType == typeof(ResourceSetupHostService))
			.ToList();

		foreach (ServiceDescriptor descriptor in resourceSetupHostServiceDescriptors)
		{
			services.Remove(descriptor);
		}
	}
}
