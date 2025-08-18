using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using HealthChecks.UI.Client;
using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ModulebankProject.Extensions;
using ModulebankProject.Features.Accounts;
using ModulebankProject.Features.Outbox;
using ModulebankProject.Features.Transactions;
using ModulebankProject.HealthCheck;
using ModulebankProject.Infrastructure.AccrueInterest;
using ModulebankProject.Infrastructure.Data;
using ModulebankProject.Infrastructure.Data.Repositories;
using ModulebankProject.Infrastructure.RabbitMq;
using ModulebankProject.Infrastructure.RabbitMq.AntifraudConsumer;
using ModulebankProject.Middlewares;
using ModulebankProject.PipelineBehaviors;
using ModulebankProject.PipelineBehaviors.Outbox;
using Npgsql;
using RabbitMQ.Client;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Swashbuckle.AspNetCore.Annotations;

namespace ModulebankProject;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .WriteTo.File(new RenderedCompactJsonFormatter(), "logs/log.json", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();

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
            if(!builder.Environment.IsEnvironment("Testing")) builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionalOutboxBehavior<,>));
            builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

            //DATA
            var connString = builder.Configuration.GetConnectionString(nameof(ModulebankDataContext));

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
            dataSourceBuilder.EnableDynamicJson();
            var dataSource = dataSourceBuilder.Build();

            builder.Services.AddDbContext<ModulebankDataContext>(options =>
                options.UseNpgsql(dataSource));
            var services = builder.Services.BuildServiceProvider();

            using (var scope = services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ModulebankDataContext>();
                db.Database.Migrate();
            }
            builder.Services.AddScoped<IAccountsRepository, AccountsRepository>();
            builder.Services.AddScoped<ITransactionsRepository, TransactionsRepository>();

            //Hangfire
            if (!builder.Environment.IsEnvironment("Testing"))
            {
                builder.Services.AddHangfire((_, config) =>
                {
                    config
                        .UsePostgreSqlStorage(
                            options =>
                            {
                                options.UseNpgsqlConnection(
                                    builder.Configuration.GetConnectionString(nameof(ModulebankDataContext)));
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

            if (!builder.Environment.IsEnvironment("Testing"))
            {
                //RabbitMQ
                builder.Services.AddSingleton<IConnection>(_ =>
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = "rabbitmq",
                        UserName = "admin",
                        Password = "admin",
                        Port = AmqpTcpEndpoint.UseDefaultPort
                    };
                    return factory.CreateConnectionAsync().Result;
                });
                builder.Services.AddSingleton<IChannel>(sp =>
                {
                    var connection = sp.GetRequiredService<IConnection>();
                    var channel = connection.CreateChannelAsync().Result;

                    ConfigureRabbitMqTopology(channel);

                    return channel;
                });
                builder.Services.AddHostedService<AntifraudConsumer>();
                builder.Services.AddHostedService<AuditConsumer>();
                builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
                builder.Services.AddScoped<IAntifraudEventHandler, AntifraudEventHandler>();
                builder.Services.AddHostedService<OutboxBackgroundService>();
            }

            builder.Services.AddAuthorization();
            if (!builder.Environment.IsEnvironment("Testing"))
            {
                builder.Services.AddAuth(builder.Configuration);
            }

            builder.Services.AddHealthChecks()
                // Базовая проверка жизнеспособности (LIVE)
                .AddCheck<BasicHealthCheck>(
                    "basic_health_check",
                    tags: new[] { "basic" })

                // Проверка подключения к RabbitMQ
                .AddRabbitMQ(
                    factory: sp =>
                        sp.GetRequiredService<IConnection>(), // Используем уже зарегистрированное подключение
                    name: "rabbitmq",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "messaging", "ready" },
                    timeout: TimeSpan.FromSeconds(3))

                // ReSharper disable once CommentTypo
                // Проверка состояния Outbox (пример для PostgreSQL)
                .AddNpgSql(
                    connectionString: builder.Configuration.GetConnectionString(nameof(ModulebankDataContext))!,
                    name: "outbox_database",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "ready", "storage" })

                // Кастомная проверка Outbox на скопление сообщений
                .AddCheck<OutboxHealthCheck>("outbox_health_check",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "ready", "messaging" });


            var app = builder.Build();

            app.UseCors("AllowAll");

            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                opt.OAuthClientId("client");
                //opt.SwaggerEndpoint("/swagger/events/swagger.json", "События");
            });
            //}
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseHttpsRedirection();
            app.MapControllers();

            app.UseAuthentication();
            app.UseAuthorization();

            if (!builder.Environment.IsEnvironment("Testing"))
            {
                app.UseHangfireDashboard();
                BackgroundJob.Enqueue(() => Console.WriteLine("Hangfire is working!"));
                RecurringJob.AddOrUpdate<InterestAccrualService>("accrueInterestJob",
                    service => service.InvokeHandler(), Cron.Daily(0, 30));
            }

            app.MapHealthChecks("/health/live", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("basic"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                })
                .WithTags("HealthChecks")
                .WithDisplayName("Liveliness Probe")
                .WithMetadata(new SwaggerOperationAttribute(
                    summary: "Basic application health check",
                    description: "Checks if the application is running (BasicHealthCheck)"));

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("ready"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                })
                .WithTags("HealthChecks")
                .WithDisplayName("Readiness Probe");

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureRabbitMqTopology(IChannel channel)
    {
        channel.ExchangeDeclareAsync("account.events", ExchangeType.Topic, durable: true);
        channel.ExchangeDeclareAsync("money.operations", ExchangeType.Topic, durable: true);
        channel.ExchangeDeclareAsync("antifraud.events", ExchangeType.Topic, durable: true);

        channel.QueueDeclareAsync("account.opened.queue", durable: true, exclusive: false, autoDelete: false);
        channel.QueueDeclareAsync("money.operations.queue", durable: true, exclusive: false, autoDelete: false);
        channel.QueueDeclareAsync("client.status.queue", durable: true, exclusive: false, autoDelete: false);

        channel.QueueBindAsync("account.opened.queue", "account.events", "account.opened");
        channel.QueueBindAsync("money.operations.queue", "money.operations", "money.*");
        channel.QueueBindAsync("client.status.queue", "antifraud.events", "client.*");
    }
}