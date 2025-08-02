using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ModulebankProject.Features.Accounts;
using ModulebankProject.Features.Transactions;
using ModulebankProject.Infrastructure.Data;
using ModulebankProject.Infrastructure.Data.Repositories;
using ModulebankProject.Middlewares;
using ModulebankProject.PipelineBehaviors;

namespace ModulebankProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen(options =>
            {
                var basePath = AppContext.BaseDirectory;

                var xmlPath = Path.Combine(basePath, "ModulebankProject.xml");
                options.UseInlineDefinitionsForEnums();
                options.IncludeXmlComments(xmlPath);

                options.CustomSchemaIds(id => id.FullName!.Replace('+','-'));

                options.AddSecurityDefinition("Keycloak", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            //AuthorizationUrl = new Uri(builder.Configuration["Keycloak:AuthUrl"]!),
                            AuthorizationUrl = new Uri($"{builder.Configuration["Keycloak:AuthUrl"]}?client_id=client&response_type=token"),
                            
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
            });

            builder.Services.AddMediatR(cfg => 
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingTransaction>();
                cfg.AddProfile<MappingAccount>();
            });
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

            //DATA
            builder.Services.AddSingleton<IMyDataContext, MyDataContext>();
            builder.Services.AddScoped<IAccountsRepository, AccountsRepository>();
            builder.Services.AddScoped<ITransactionsRepository, TransactionsRepository>();

            //
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = false;
                    o.Audience = builder.Configuration["Authentication:Audience"];
                    o.MetadataAddress = builder.Configuration["Authentication:MetadataAddress"]!;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = builder.Configuration["Authentication:ValidIssuer"],
                    };
                });

            var app = builder.Build();

            app.UseCors("AllowAll");

            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI(c => { 
                    c.OAuthClientId("client");
                });
            //}
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseHttpsRedirection();
            app.MapControllers();

            app.UseAuthentication();
            app.UseAuthorization();

            app.Run();
        }
    }
}
