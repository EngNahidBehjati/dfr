using DataFlowRunner.DataFlowChangeEvents.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.Tracking;

namespace DataFlowRunner.Integration.Tests.DataFlowChangeEvents.Helpers;

internal sealed class DataFlowChangeEventPublishHelper
{
	private const int TrackingTimeoutInSeconds = 20;
	private readonly WebApplicationFactory<Program> _factory;

	public DataFlowChangeEventPublishHelper(WebApplicationFactory<Program> factory)
	{
		_factory = factory ?? throw new ArgumentNullException(nameof(factory));
	}

	public async Task<ITrackedSession> PublishAndWaitAsync(
		Guid dataFlowExecutionGuid,
		ChangeEvent changeEvent)
	{
		IHost host = _factory.Services.GetRequiredService<IHost>();
		var options = new DeliveryOptions
		{
			PartitionKey = dataFlowExecutionGuid.ToString()
		};

		return await host.Services
			.TrackActivity()
			.Timeout(TimeSpan.FromSeconds(TrackingTimeoutInSeconds))
			.ExecuteAndWaitAsync(context => context.SendAsync(changeEvent, options));
	}
}
