using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ModulebankProject.Infrastructure.Data;

namespace ModulebankProject.Tests.Base;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
{
    protected readonly ISender Sender;
    protected readonly ModulebankDataContext DbContext;
    protected readonly HttpClient Client;

    // ReSharper disable once PublicConstructorInAbstractClass не хочу первичный конструктор
    public BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        //_scope = factory.Services.CreateScope();
        var scope = factory.Server.Services.CreateScope();

        Sender = scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = scope.ServiceProvider.GetRequiredService<ModulebankDataContext>();
        Client = factory.CreateClient();
    }
}