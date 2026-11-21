using System.Net.Security;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.Common.DataFlowBuilderConfiguration.Options;
using Microsoft.Extensions.Options;

namespace DataFlowRunner.Common.Http.Extensions;

internal static class HttpClientBuilderCertificateExtensions
{
	public static IHttpClientBuilder AddAccessManagementCertificateValidation(this IHttpClientBuilder builder)
	{
		return builder.ConfigurePrimaryHttpMessageHandler(serviceProvider =>
		{
			var thumbprint = serviceProvider.GetRequiredService<IOptions<AmConfig>>().Value.EngineCertHash;
			return AddCertificateThumbprintValidation(thumbprint);
		});
	}

	public static IHttpClientBuilder AddDataFlowBuilderCertificateValidation(this IHttpClientBuilder builder)
	{
		return builder.ConfigurePrimaryHttpMessageHandler(serviceProvider =>
		{
			var thumbprint = serviceProvider.GetRequiredService<IOptions<DataFlowBuilderEndpoint>>().Value
				.EngineCertHash;
			return AddCertificateThumbprintValidation(thumbprint);
		});
	}

	private static HttpClientHandler AddCertificateThumbprintValidation(string thumbprint)
	{
		var handler = new HttpClientHandler();
		if (!string.IsNullOrWhiteSpace(thumbprint))
		{
			handler.ServerCertificateCustomValidationCallback = (_, certificate, _, errors) =>
			{
				if (errors == SslPolicyErrors.None)
				{
					return true;
				}

				return certificate is not null &&
				       string.Equals(certificate.Thumbprint, thumbprint, StringComparison.InvariantCultureIgnoreCase);
			};
		}

		return handler;
	}
}
