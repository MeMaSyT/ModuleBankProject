using FluentValidation;
using MediatR;
using ModulebankProject.Extensions;
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
            builder.Services.AddSingleton<IMyDataContext, MyDataContext>();
            builder.Services.AddScoped<IAccountsRepository, AccountsRepository>();
            builder.Services.AddScoped<ITransactionsRepository, TransactionsRepository>();

            //
            builder.Services.AddAuthorization();
            builder.Services.AddAuth(builder.Configuration);

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

            app.Run();
        }
    }
}
