using FluentValidation;
using MediatR;
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

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen();

            builder.Services.AddMediatR(cfg => 
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
            //builder.Services.AddAutoMapper(_ => { }, typeof(MappingAccount), typeof(MappingTransaction));
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

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseHttpsRedirection();
            app.MapControllers();

            app.Run();
        }
    }
}
