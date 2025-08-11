using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulebankProject.Extensions;
using ModulebankProject.Features.Accounts;
using ModulebankProject.Features.Transactions;
using ModulebankProject.Infrastructure.AccrueInterest;
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
            builder.Services.AddCustomSwagger(builder.Configuration);

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
            builder.Services.AddDbContext<ModulebankDataContext>(
                options =>
                {
                    options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(ModulebankDataContext)));
                });
            builder.Services.AddScoped<IAccountsRepository, AccountsRepository>();
            builder.Services.AddScoped<ITransactionsRepository, TransactionsRepository>();

            //Hangfire
            if (!builder.Environment.IsEnvironment("Testing"))
            {
                builder.Services.AddHangfire((_, config) =>
                {
                    config
                        .UsePostgreSqlStorage(options =>
                            {
                                options.UseNpgsqlConnection(builder.Configuration.GetConnectionString(nameof(ModulebankDataContext)));
                            },
                            new PostgreSqlStorageOptions
                            {
                                SchemaName = "hangfire",
                                PrepareSchemaIfNecessary = true,
                                EnableTransactionScopeEnlistment = true,
                                UseNativeDatabaseTransactions = true
                            });
                });
                builder.Services.AddHangfireServer();
            }
            builder.Services.AddScoped<AccrueInterestHandler>();
            builder.Services.AddScoped<InterestAccrualService>();

            builder.Services.AddAuthorization();
            if (!builder.Environment.IsEnvironment("Testing"))
            {
                builder.Services.AddAuth(builder.Configuration);
            }

            var app = builder.Build();

            app.UseCors("AllowAll");

            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI(opt => opt.OAuthClientId("client"));
            //}
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseHttpsRedirection();
            app.MapControllers();

            app.UseAuthentication();
            app.UseAuthorization();

            if (!builder.Environment.IsEnvironment("Testing"))
            {
                app.UseHangfireDashboard();
                BackgroundJob.Enqueue(() => Console.WriteLine("Hangfire is working!"));
                RecurringJob.AddOrUpdate<InterestAccrualService>("accrueInterestJob", service => service.InvokeHandler(), Cron.Daily(0, 30));
            }

            app.Run();
        }
    }
}
