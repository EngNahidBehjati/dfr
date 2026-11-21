using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace DataFlowRunner.Common.HealthChecks;

internal static class HealthCheckMapper
{
	public static void MapHealthChecks(this WebApplication app)
	{
		app.MapHealthChecks("api/health", new HealthCheckOptions
		{
			ResponseWriter = async (context, report) =>
			{
				context.Response.ContentType = "application/json";
				var result = new
				{
					status = report.Status.ToString(),
					checks = report.Entries.Select(e => new
					{
						name = e.Key,
						status = e.Value.Status.ToString(),
						description = e.Value.Description,
						error = e.Value.Exception?.Message
					})
				};
				await context.Response.WriteAsync(JsonSerializer.Serialize(result))
					.ConfigureAwait(false);
			}
		}).AllowAnonymous();
	}
}
