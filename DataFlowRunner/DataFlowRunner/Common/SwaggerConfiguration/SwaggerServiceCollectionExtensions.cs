using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DataFlowRunner.Common.SwaggerConfiguration;

internal static class SwaggerServiceCollectionExtensions
{
	private const string AuthTokenSchemeId = "AuthToken";

	public static void AddSwaggerWithAuthToken(this IServiceCollection services)
	{
		services.AddSwaggerGen(options =>
		{
			ConfigureAuthTokenSecurityDefinition(options);
			ConfigureAuthTokenSecurityRequirement(options);
		});
	}

	private static void ConfigureAuthTokenSecurityDefinition(SwaggerGenOptions options)
	{
		options.AddSecurityDefinition(AuthTokenSchemeId, new OpenApiSecurityScheme
		{
			Name = "Cookie",
			Type = SecuritySchemeType.ApiKey,
			In = ParameterLocation.Header,
			Description = $"Enter your auth token here"
		});
	}

	private static void ConfigureAuthTokenSecurityRequirement(SwaggerGenOptions options)
	{
		options.AddSecurityRequirement(new OpenApiSecurityRequirement
		{
			{
				new OpenApiSecurityScheme
				{
					Reference = new OpenApiReference
					{
						Type = ReferenceType.SecurityScheme,
						Id = AuthTokenSchemeId
					}
				},
				Array.Empty<string>()
			}
		});
	}
}
