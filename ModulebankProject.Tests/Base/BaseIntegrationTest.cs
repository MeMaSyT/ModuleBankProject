using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ModulebankProject.Infrastructure.Data;

namespace ModulebankProject.Tests.Base
{
    public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
    {
        protected readonly ISender Sender;
        protected readonly ModulebankDataContext DbContext;
        protected readonly HttpClient Client;
        private readonly IServiceScope _scope;
        // ReSharper disable once PublicConstructorInAbstractClass не хочу первичный конструктор
        public BaseIntegrationTest(IntegrationTestWebAppFactory factory)
        {
            //_scope = factory.Services.CreateScope();
            _scope = factory.Server.Services.CreateScope();

            Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
            DbContext = _scope.ServiceProvider.GetRequiredService<ModulebankDataContext>();
            Client = factory.CreateClient();
        }
    }
}
