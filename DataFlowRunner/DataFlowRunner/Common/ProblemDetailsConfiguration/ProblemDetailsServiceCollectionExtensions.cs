using System.Diagnostics;
using DataFlowRunner.Language.LanguageDictionary;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace DataFlowRunner.Common.ProblemDetailsConfiguration;


internal static class ProblemDetailsServiceCollectionExtensions
{
	/// <summary>
	/// Configures custom <see cref="ProblemDetails"/> behavior:
	/// <list type="bullet">
	///   <item>
	///     <description>
	///       Sets the HTTP response status code from <c>ProblemDetails.Status</c> (if present).
	///     </description>
	///   </item>
	///   <item>
	///     <description>
	///       Adds <c>requestId</c> (<c>HttpContext.TraceIdentifier</c>) to <c>ProblemDetails.Extensions</c>.
	///     </description>
	///   </item>
	///   <item>
	///     <description>
	///       Adds <c>traceId</c> (current <see cref="Activity"/> TraceId, if available) to <c>ProblemDetails.Extensions</c>.
	///     </description>
	///   </item>
	/// </list>
	/// </summary>
	public static void AddCustomProblemDetails(this IServiceCollection services)
	{
		services.AddProblemDetails(options =>
		{
			options.CustomizeProblemDetails = context =>
			{
				if (context.ProblemDetails.Status.HasValue)
				{
					context.HttpContext.Response.StatusCode = context.ProblemDetails.Status.Value;
				}

				context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
				Activity? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
				if (activity?.Id is not null)
				{
					context.ProblemDetails.Extensions.TryAdd("traceId", activity.Id);
				}

				if (string.IsNullOrEmpty(context.ProblemDetails.Title))
				{
					context.ProblemDetails.Title =
						LanguageDictionaryProvider.Instance.DataFlowRunnerLanguage.DataFlowRunner.Common.DefaultError();
				}
			};
		});
	}
}
