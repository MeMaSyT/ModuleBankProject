using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ModulebankProject.Features.Inbox.Events;
using ModulebankProject.HealthCheck;
using Swashbuckle.AspNetCore.Filters;

namespace ModulebankProject.Extensions
{
    public static class ApiExtensions
    {
        public static IServiceCollection AddCustomSwagger(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOpenApi();
            services.AddSwaggerExamplesFromAssemblyOf<Program>();
            services.AddSwaggerGen(options =>
            {
                var basePath = AppContext.BaseDirectory;

                var xmlPath = Path.Combine(basePath, "ModulebankProject.xml");
                options.UseInlineDefinitionsForEnums();
                options.IncludeXmlComments(xmlPath);

                options.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));

                options.AddSecurityDefinition("Keycloak", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(configuration["Keycloak:AuthUrl"]!),

                            /*Scopes = new Dictionary<string, string>
                            {
                                {"openid", "openid"},
                                {"profile", "profile"}
                            }*/
                        }
                    }
                });

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Keycloak",
                                Type = ReferenceType.SecurityScheme
                            },
                            In = ParameterLocation.Header,
                            Name = "Bearer",
                            Scheme = "Bearer"
                        },
                        []
                    }
                };
                options.AddSecurityRequirement(securityRequirement);

                options.DocumentFilter<HealthCheckDocumentFilter>();

                options.ExampleFilters();
                options.DocumentFilter<EventDocumentFilter>();
            });
            return services;
        }
        public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = false;
                    o.Audience = configuration["Authentication:Audience"];
                    o.MetadataAddress = configuration["Authentication:MetadataAddress"]!;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = configuration["Authentication:ValidIssuer"],
                    };
                });
            return services;
        }
    }
}
